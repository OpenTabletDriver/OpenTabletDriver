using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using Octokit;
using OpenTabletDriver.ComponentProviders;
using OpenTabletDriver.Components;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Daemon.Components;
using OpenTabletDriver.Daemon.Diagnostics;
using OpenTabletDriver.Daemon.Interop;
using OpenTabletDriver.Daemon.Reflection;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Daemon
{
    using static ServiceDescriptor;

    public class DesktopServiceCollection : ServiceCollection
    {
        public DesktopServiceCollection()
        {
            this.AddServices(new[]
            {
                // Core Services
                Singleton<SynchronizationContext>(_ => new NonConcurrentSynchronizationContext(false)),
                Singleton<IDriver, Driver>(),
                Singleton<IReportParserProvider, ReportParserProvider>(),
                Singleton<IDeviceHubsProvider, DeviceHubsProvider>(p => new DeviceHubsProvider(p)),
                Singleton<ICompositeDeviceHub, CompositeDeviceHub>(CompositeDeviceHub.WithProvider),
                Singleton<IDeviceConfigurationProvider, DesktopDeviceConfigurationProvider>(),
                Singleton<IReportParserProvider, DesktopReportParserProvider>(),
                // Desktop Services
                Singleton<IGitHubClient>(new GitHubClient(ProductHeaderValue.Parse("OpenTabletDriver"))),
                Transient<EnvironmentDictionary, EnvironmentDictionary>(),
                Singleton<IPluginManager, PluginManager>(),
                Singleton<ISettingsPersistenceManager, SettingsPersistenceManager>(),
                Singleton<IPresetManager, PresetManager>(),
                Singleton<IPluginFactory, PluginFactory>(),
                Singleton<ISleepDetector, SleepDetector>(),
            });
        }

        public static DesktopServiceCollection GetPlatformServiceCollection()
        {
            return SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => new DesktopWindowsServiceCollection(),
                SystemPlatform.Linux => new DesktopLinuxServiceCollection(),
                SystemPlatform.MacOS => new DesktopMacOSServiceCollection(),
                _ => throw new PlatformNotSupportedException("This platform is not supported by OpenTabletDriver.")
            };
        }
    }
}
