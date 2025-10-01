using System;

namespace Jellyfin.Plugin.Template.Configuration;

/// <summary>
/// Represents a retention rule for a given library folder.
/// </summary>
public class RetentionRule
{
    /// <summary>
    /// Gets or sets the Jellyfin library folder id.
    /// </summary>
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the display name shown in the configuration UI.
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum retention in days.
    /// </summary>
    public int RetentionDays { get; set; }
}
