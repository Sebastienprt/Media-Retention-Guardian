namespace MediaRetentionGuardian.Controllers;

/// <summary>
/// DTO returned by the drive info endpoint.
/// </summary>
public class DriveInfoResponse
{
    /// <summary>
    /// Gets or sets the resolved root path.
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// Gets or sets the available free space in bytes.
    /// </summary>
    public long? AvailableBytes { get; set; }

    /// <summary>
    /// Gets or sets the total size in bytes.
    /// </summary>
    public long? TotalBytes { get; set; }
}
