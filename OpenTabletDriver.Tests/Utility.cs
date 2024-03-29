using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;
using OpenTabletDriver.Daemon.Interop.Timer;
using OpenTabletDriver.Daemon.Reflection;

namespace OpenTabletDriver.Tests
{
    public static class Utility
    {
        public static IServiceCollection GetServices()
        {
            return new DesktopServiceCollection()
                .AddSingleton<IPluginManager, PluginManager>()
                .AddTransient<ITimer, FallbackTimer>()
                .AddSingleton<IAppInfo, AppInfo>();
        }
    }
}
