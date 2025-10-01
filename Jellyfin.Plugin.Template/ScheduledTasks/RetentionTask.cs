using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Template.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Template.ScheduledTasks;

/// <summary>
/// Scheduled task that enforces the configured retention rules.
/// </summary>
public class RetentionTask : IScheduledTask, IConfigurableScheduledTask
{
    private static readonly BaseItemKind[] IncludedItemKinds =
    {
        BaseItemKind.Episode,
        BaseItemKind.Season,
        BaseItemKind.Series,
        BaseItemKind.Movie,
        BaseItemKind.Video,
        BaseItemKind.Audio,
        BaseItemKind.MusicVideo
    };

    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<RetentionTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetentionTask"/> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    public RetentionTask(ILibraryManager libraryManager, ILogger<RetentionTask> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Category => Plugin.Instance?.Name ?? "Media Retention";

    /// <inheritdoc />
    public string Key => "MediaRetentionAutoCleanup";

    /// <inheritdoc />
    public string Name => "Media retention cleanup";

    /// <inheritdoc />
    public string Description => "Deletes media files that exceed the configured retention period.";

    /// <inheritdoc />
    public bool IsHidden => false;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsLogged => true;

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(3).Ticks
            }
        ];
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance;
        if (plugin == null)
        {
            _logger.LogWarning("Plugin instance unavailable; skipping execution.");
            return;
        }

        var rules = plugin.Configuration.RetentionRules;
        if (rules == null || rules.Count == 0)
        {
            _logger.LogInformation("No retention rules configured.");
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var processedFolders = 0;
        foreach (var rule in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processedFolders++;
            progress?.Report(processedFolders / (double)rules.Count);

            if (rule.RetentionDays <= 0)
            {
                _logger.LogInformation("Skipping folder {FolderName} because retention is not greater than zero.", rule.FolderName);
                continue;
            }

            if (_libraryManager.GetItemById(rule.FolderId) is not Folder folder)
            {
                _logger.LogWarning("Folder with id {FolderId} not found.", rule.FolderId);
                continue;
            }

            var retentionSpan = TimeSpan.FromDays(rule.RetentionDays);
            var cutoff = nowUtc - retentionSpan;

            _logger.LogInformation("Evaluating folder {FolderName} ({FolderId}) with retention {RetentionDays} days (cutoff {Cutoff}).", rule.FolderName, rule.FolderId, rule.RetentionDays, cutoff.ToString("u", CultureInfo.InvariantCulture));

            var items = _libraryManager.GetItemList(new InternalItemsQuery
            {
                AncestorIds = new[] { folder.Id },
                Recursive = true,
                IsVirtualItem = false,
                IncludeItemTypes = IncludedItemKinds
            });

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var referenceDate = item.DateModified;
                if (referenceDate == default)
                {
                    referenceDate = item.DateCreated;
                }

                if (referenceDate == default)
                {
                    continue;
                }

                var referenceUtc = referenceDate.Kind == DateTimeKind.Utc ? referenceDate : referenceDate.ToUniversalTime();

                if (referenceUtc <= cutoff && !string.IsNullOrEmpty(item.Path) && File.Exists(item.Path))
                {
                    try
                    {
                        File.Delete(item.Path);
                        _logger.LogInformation("Deleted item {ItemName} at path {ItemPath}.", item.Name, item.Path);

                        plugin.AppendDeletionRecord(new DeletionRecord
                        {
                            ItemId = item.Id,
                            Name = item.Name,
                            MediaType = item.GetType().Name,
                            DeletedAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete path {Path} for item {ItemName}.", item.Path, item.Name);
                    }
                }
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
