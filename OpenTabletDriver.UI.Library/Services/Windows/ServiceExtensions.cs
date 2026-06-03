using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.UI.Services.Windows;

public static class ServiceExtensions
{
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddWindowsServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAutoStartService, ShortcutAutoStartService>()
            .AddSingleton<IDriverDaemonAutoStartService, TaskSchedulerDaemonAutoStartService>();
    }
}
