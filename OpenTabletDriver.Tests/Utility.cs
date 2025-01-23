using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Library;
using OpenTabletDriver.Daemon.Library.Interop.Timer;
using OpenTabletDriver.Daemon.Library.Reflection;

namespace OpenTabletDriver.Tests
{
    public static class Utility
    {
        public static IServiceCollection GetServices()
        {
            return new DesktopServiceCollection()
                .AddSingleton<IPluginManager, PluginManager>()
                .AddTransient<ITimer, FallbackTimer>();
        }
    }
}
