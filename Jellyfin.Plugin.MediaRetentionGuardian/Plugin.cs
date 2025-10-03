using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaRetentionGuardian.Configuration;

namespace MediaRetentionGuardian;

/// <summary>
/// Main entry point for the Media Retention Guardian plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Provides application directories.</param>
    /// <param name="xmlSerializer">Serializer used for configuration.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Media Retention Guardian";

    /// <inheritdoc />
    public override string Description => "Automatise la suppression des fichiers dépassant la durée de rétention configurée.";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("5c5422c9-396b-4dec-87fa-a8a20f65c549");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            },
            new PluginPageInfo
            {
                Name = "MediaRetentionGuardianConfigCss",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.css", GetType().Namespace)
            }
        ];
    }

    /// <inheritdoc />
    public override PluginInfo GetPluginInfo()
    {
        var info = base.GetPluginInfo();
        info.HasImage = true;
        return info;
    }

    /// <summary>
    /// Opens the bundled thumbnail image if present.
    /// </summary>
    /// <returns>Stream of the thumbnail image or null.</returns>
    public Stream? OpenThumbImage()
    {
        var pluginDir = Path.GetDirectoryName(AssemblyFilePath);
        if (string.IsNullOrEmpty(pluginDir))
        {
            return null;
        }

        var thumbPath = Path.Combine(pluginDir, "thumb.png");
        return File.Exists(thumbPath) ? File.OpenRead(thumbPath) : null;
    }
}
