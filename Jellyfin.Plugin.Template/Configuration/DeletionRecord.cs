using System;

namespace Jellyfin.Plugin.Template.Configuration;

/// <summary>
/// Represents a single deletion notification displayed in the UI.
/// </summary>
public class DeletionRecord
{
    /// <summary>
    /// Gets or sets the item id deleted.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the display title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media type (Episode, Movie, etc.).
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time the deletion occurred in UTC.
    /// </summary>
    public DateTime DeletedAt { get; set; }
}
