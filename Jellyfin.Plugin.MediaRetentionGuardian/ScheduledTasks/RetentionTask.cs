using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller;
using MediaBrowser.Model.Tasks;
using MediaRetentionGuardian.Services;
using Microsoft.Extensions.Logging;

namespace MediaRetentionGuardian.ScheduledTasks;

/// <summary>
/// Scheduled task that performs retention clean-up for configured targets.
/// </summary>
public sealed class RetentionTask : IScheduledTask
{
    private readonly ILogger<RetentionTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetentionTask"/> class.
    /// </summary>
    /// <param name="logger">Typed logger.</param>
    public RetentionTask(ILogger<RetentionTask> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Key => "MediaRetentionGuardian.Retention";

    /// <inheritdoc />
    public string Name => "Media Retention Guardian";

    /// <inheritdoc />
    public string Description => "Supprime les fichiers dépassant la durée de conservation configurée.";

    /// <inheritdoc />
    public string Category => "Media Retention Guardian";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfo.TriggerDaily,
            TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
        };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);

        var service = new RetentionCleanupService(_logger);

        try
        {
            var result = await service.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation(
                "Retention clean-up completed. Deleted {Deleted} files across {Targets} targets.",
                result.DeletedFiles,
                result.TargetResults.Count);

            foreach (var target in result.TargetResults)
            {
                if (target.Threshold.HasValue)
                {
                    _logger.LogInformation(
                        "Target {Path} ({Days} jours, seuil {Threshold}%) - {Deleted} fichier(s) supprimé(s)",
                        target.Path,
                        target.Days,
                        target.Threshold.Value,
                        target.DeletedFiles);
                }
                else
                {
                    _logger.LogInformation(
                        "Target {Path} ({Days} jours) - {Deleted} fichier(s) supprimé(s)",
                        target.Path,
                        target.Days,
                        target.DeletedFiles);
                }

                foreach (var error in target.Errors)
                {
                    _logger.LogWarning("Retention error for {Path}: {Error}", target.Path, error);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Retention clean-up cancelled.");
            throw;
        }
        finally
        {
            progress.Report(100);
        }
    }
}
