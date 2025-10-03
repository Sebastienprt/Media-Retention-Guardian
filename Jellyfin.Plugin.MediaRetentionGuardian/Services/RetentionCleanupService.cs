using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaRetentionGuardian.Configuration;
using Microsoft.Extensions.Logging;

namespace MediaRetentionGuardian.Services;

/// <summary>
/// Provides retention clean-up logic for configured targets.
/// </summary>
internal sealed class RetentionCleanupService
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        ReturnSpecialDirectories = false
    };

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetentionCleanupService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public RetentionCleanupService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the retention clean-up process.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result summary.</returns>
    public Task<RetentionCleanupResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin is null)
        {
            _logger.LogError("Media Retention Guardian plugin instance is unavailable. Skipping clean-up.");
            return Task.FromResult(new RetentionCleanupResult(0, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture), Array.Empty<RetentionTargetResult>()));
        }

        var configuration = plugin.Configuration;
        if (configuration is null)
        {
            _logger.LogError("Media Retention Guardian configuration is unavailable. Skipping clean-up.");
            return Task.FromResult(new RetentionCleanupResult(0, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture), Array.Empty<RetentionTargetResult>()));
        }

        var targets = configuration.RetentionTargets ?? new List<RetentionTarget>();
        var targetResults = new List<RetentionTargetResult>(targets.Count);
        var totalDeleted = 0;

        _logger.LogInformation("Starting retention clean-up for {TargetCount} target(s).", targets.Count);

        var useThresholds = configuration.EnableDiskThreshold;
        var triggeredByThreshold = false;

        foreach (var target in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = ProcessTarget(target, useThresholds, cancellationToken);
            totalDeleted += result.DeletedFiles;
            targetResults.Add(result);

            if (result.Threshold.HasValue && result.ThresholdSatisfied)
            {
                triggeredByThreshold = true;
            }
        }

        var formattedTimestamp = DateTime.Now.ToString("dd/MM/yyyy - HH:mm", CultureInfo.InvariantCulture);

        configuration.LastRunDeletedCount = totalDeleted;
        configuration.LastRunUtc = formattedTimestamp;
        configuration.LastRunTriggeredByDiskThreshold = triggeredByThreshold;

        try
        {
            plugin.UpdateConfiguration(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist Media Retention Guardian configuration.");
        }

        return Task.FromResult(new RetentionCleanupResult(totalDeleted, formattedTimestamp, targetResults));
    }

    private RetentionTargetResult ProcessTarget(RetentionTarget target, bool useThresholds, CancellationToken cancellationToken)
    {
        var result = new RetentionTargetResult(target.Path, target.Days);

        if (string.IsNullOrWhiteSpace(target.Path))
        {
            _logger.LogDebug("Skipping empty retention target.");
            return result;
        }

        if (!Directory.Exists(target.Path))
        {
            _logger.LogWarning("Retention path does not exist: {RetentionPath}", target.Path);
            result.Errors.Add($"Le dossier '{target.Path}' n'existe pas.");
            return result;
        }

        if (useThresholds)
        {
            var threshold = target.TriggerFreeSpacePercent;
            if (threshold.HasValue)
            {
                var thresholdValue = Math.Clamp(threshold.Value, 1, 100);
                result.Threshold = thresholdValue;

                var driveInfo = TryResolveDrive(target.Path);
                if (driveInfo == null || !driveInfo.IsReady || driveInfo.TotalSize <= 0)
                {
                    var message = $"Impossible d'évaluer l'espace disque pour '{target.Path}'.";
                    _logger.LogWarning("Unable to evaluate disk space for {Path}.", target.Path);
                    result.Errors.Add(message);
                    return result;
                }

                var freePercent = (double)driveInfo.AvailableFreeSpace / driveInfo.TotalSize * 100;
                if (freePercent > thresholdValue)
                {
                    _logger.LogInformation(
                        "Skipping retention for {Path} (free space {FreePercent:F1}% exceeds threshold {Threshold}%).",
                        target.Path,
                        freePercent,
                        thresholdValue);
                    result.Errors.Add($"Ignoré (espace libre {freePercent:F1}% > seuil {thresholdValue}%).");
                    return result;
                }

                _logger.LogInformation(
                    "Free space {FreePercent:F1}% <= threshold {Threshold}%, retention will proceed for {Path}.",
                    freePercent,
                    thresholdValue,
                    target.Path);

                result.ThresholdSatisfied = true;
            }
            else
            {
                result.Threshold = null;
                result.ThresholdSatisfied = false;
            }
        }
        else
        {
            result.Threshold = null;
            result.ThresholdSatisfied = false;
        }

        var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, target.Days));

        try
        {
            foreach (var file in Directory.EnumerateFiles(target.Path, "*", EnumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var lastWrite = File.GetLastWriteTimeUtc(file);
                    if (lastWrite <= cutoff)
                    {
                        File.Delete(file);
                        result.DeletedFiles++;
                        _logger.LogInformation("Deleted expired file {File}", file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {File}", file);
                    result.Errors.Add($"{file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to enumerate files for retention path {RetentionPath}", target.Path);
            result.Errors.Add($"Impossible de parcourir le dossier '{target.Path}'.");
        }

        return result;
    }

    internal static DriveInfo? TryResolveDrive(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            if (root.StartsWith(@"\\", StringComparison.Ordinal))
            {
                try
                {
                    return new DriveInfo(root);
                }
                catch
                {
                    return null;
                }
            }

            DriveInfo? bestMatch = null;
            foreach (var drive in DriveInfo.GetDrives())
            {
                var driveRoot = drive.RootDirectory.FullName;
                if (path.StartsWith(driveRoot, StringComparison.OrdinalIgnoreCase))
                {
                    if (bestMatch == null || driveRoot.Length > bestMatch.RootDirectory.FullName.Length)
                    {
                        bestMatch = drive;
                    }
                }
            }

            return bestMatch ?? new DriveInfo(root);
        }
        catch
        {
            return null;
        }
    }
}
