using System.Collections.Generic;

namespace MediaRetentionGuardian.Services;

/// <summary>
/// Represents the result of handling a single retention target.
/// </summary>
internal sealed class RetentionTargetResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetentionTargetResult"/> class.
    /// </summary>
    /// <param name="path">Target path.</param>
    /// <param name="days">Retention days.</param>
    public RetentionTargetResult(string path, int days)
    {
        Path = path;
        Days = days;
        Errors = new List<string>();
        Threshold = null;
        ThresholdSatisfied = false;
    }

    /// <summary>
    /// Gets the path processed.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the retention days applied.
    /// </summary>
    public int Days { get; }

    /// <summary>
    /// Gets or sets the number of deleted files.
    /// </summary>
    public int DeletedFiles { get; internal set; }

    /// <summary>
    /// Gets the list of errors encountered.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Gets or sets the free space threshold applied, if any.
    /// </summary>
    public int? Threshold { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether the threshold condition was satisfied (cleanup executed under the threshold).
    /// </summary>
    public bool ThresholdSatisfied { get; internal set; }
}
