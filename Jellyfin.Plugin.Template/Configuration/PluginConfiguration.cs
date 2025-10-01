using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Template.Configuration;

/// <summary>
/// Configuration container for the media retention plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the retention rules mapped by library folder.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Collection is serialized/deserialized by the Jellyfin configuration infrastructure.")]
    public IList<RetentionRule> RetentionRules { get; set; } = new List<RetentionRule>();

    /// <summary>
    /// Gets or sets the rolling log of deletions used by the UI.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Collection is serialized/deserialized by the Jellyfin configuration infrastructure.")]
    public IList<DeletionRecord> DeletionLog { get; set; } = new List<DeletionRecord>();
}
