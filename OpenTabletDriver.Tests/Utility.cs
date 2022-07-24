using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Reflection;

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
