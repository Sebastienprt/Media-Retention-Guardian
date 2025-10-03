using System.Collections.Generic;

namespace MediaRetentionGuardian.Services;

/// <summary>
/// Represents a summary of a retention clean-up execution.
/// </summary>
internal sealed class RetentionCleanupResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetentionCleanupResult"/> class.
    /// </summary>
    /// <param name="deletedFiles">Number of deleted files.</param>
    /// <param name="timestampUtc">Timestamp of execution in ISO format.</param>
    /// <param name="targets">Results for each processed target.</param>
    public RetentionCleanupResult(int deletedFiles, string timestampUtc, IReadOnlyList<RetentionTargetResult> targets)
    {
        DeletedFiles = deletedFiles;
        TimestampUtc = timestampUtc;
        TargetResults = targets;
    }

    /// <summary>
    /// Gets the total number of deleted files.
    /// </summary>
    public int DeletedFiles { get; }

    /// <summary>
    /// Gets the execution timestamp in ISO format.
    /// </summary>
    public string TimestampUtc { get; }

    /// <summary>
    /// Gets the per-target results.
    /// </summary>
    public IReadOnlyList<RetentionTargetResult> TargetResults { get; }
}
