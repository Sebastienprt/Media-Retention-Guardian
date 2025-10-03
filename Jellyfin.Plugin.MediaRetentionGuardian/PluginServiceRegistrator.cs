using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using MediaRetentionGuardian.Controllers;
using MediaRetentionGuardian.ScheduledTasks;
using Microsoft.Extensions.DependencyInjection;

namespace MediaRetentionGuardian;

/// <summary>
/// Registers plugin services with the Jellyfin host.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IScheduledTask, RetentionTask>();
        serviceCollection.AddSingleton<DriveInfoController>();
    }
}
