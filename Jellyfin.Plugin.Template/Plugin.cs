using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.Template.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Template;

/// <summary>
/// Main entry point for the media retention plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
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
    public override string Name => "Media Retention";

    /// <inheritdoc />
    public override string Description => "Automatically purge library items that are older than the configured retention period.";

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
            }
        ];
    }

    /// <summary>
    /// Adds a deletion record to the rolling log and persists the configuration.
    /// </summary>
    /// <param name="record">The deletion record.</param>
    public void AppendDeletionRecord(DeletionRecord record)
    {
        var configuration = Configuration;
        configuration.DeletionLog ??= new List<DeletionRecord>();
        configuration.DeletionLog.Insert(0, record);
        while (configuration.DeletionLog.Count > 10)
        {
            configuration.DeletionLog.RemoveAt(configuration.DeletionLog.Count - 1);
        }

        SaveConfiguration();
    }

    /// <summary>
    /// Persists retention rules returned from the configuration UI.
    /// </summary>
    /// <param name="rules">The updated set of rules.</param>
    public void UpdateRetentionRules(IList<RetentionRule> rules)
    {
        var configuration = Configuration;
        configuration.RetentionRules ??= new List<RetentionRule>();
        configuration.RetentionRules.Clear();
        foreach (var rule in rules)
        {
            configuration.RetentionRules.Add(rule);
        }

        SaveConfiguration();
    }
}
