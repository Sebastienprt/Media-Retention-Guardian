namespace MediaRetentionGuardian.Configuration;

/// <summary>
/// Represents a single retention target entry.
/// </summary>
public class RetentionTarget
{
    /// <summary>
    /// Gets or sets the filesystem path monitored for retention.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of days to keep files within the path.
    /// </summary>
    public int Days { get; set; } = 30;

    /// <summary>
    /// Gets or sets the free disk space percentage threshold that must be reached before cleanup runs.
    /// </summary>
    public int? TriggerFreeSpacePercent { get; set; }
}
