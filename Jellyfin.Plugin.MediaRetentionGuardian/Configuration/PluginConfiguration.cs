using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediaBrowser.Model.Plugins;

namespace MediaRetentionGuardian.Configuration;

/// <summary>
/// Configuration for Media Retention Guardian.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        EnableRetention = false;
        EnableDiskThreshold = false;
        RetentionPath = string.Empty;
        RetentionDays = 30;
        RetentionTargets = new List<RetentionTarget>();
        LastRunDeletedCount = -1;
        LastRunTriggeredByDiskThreshold = false;
    }

    /// <summary>
    /// Gets or sets a value indicating whether automatic retention is enabled.
    /// </summary>
    public bool EnableRetention { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether retention should consider disk free space thresholds.
    /// </summary>
    public bool EnableDiskThreshold { get; set; }

    /// <summary>
    /// Gets or sets the path that will be processed by the retention job.
    /// </summary>
    public string RetentionPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the retention duration in days.
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the list of retention targets.
    /// </summary>
    [SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Serialized to plugin configuration")]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Setter required for JSON deserialization")]
    public List<RetentionTarget> RetentionTargets { get; set; }

    /// <summary>
    /// Gets or sets the number of files deleted during the last run.
    /// </summary>
    public int LastRunDeletedCount { get; set; } = -1;

    /// <summary>
    /// Gets or sets the timestamp of the last run (UTC string).
    /// </summary>
    public string? LastRunUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the last run was triggered by disk threshold conditions.
    /// </summary>
    public bool LastRunTriggeredByDiskThreshold { get; set; }
}
