using System;
using System.Diagnostics;
using Autofac;
using JetBrains.Annotations;
using Octokit;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop
{
    public class DesktopInterop : SystemInterop
    {
        protected DesktopInterop()
        {
        }

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start explorer \"{path.Replace("&", "^&")}\"")
                    {
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    break;
                case PluginPlatform.Linux:
                    Process.Start("xdg-open", $"\"{path}\"");
                    break;
                case PluginPlatform.MacOS:
                case PluginPlatform.FreeBSD:
                    Process.Start("open", $"\"{path}\"");
                    break;
            }
        }

        public static void OpenFolder(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    Process.Start("explorer", $"\"{path.Replace("&", "^&")}\"");
                    break;
                default:
                    Open(path);
                    break;
            }
        }

        public static IGitHubClient GitHubClient => new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

        public static IUpdater? Updater => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsUpdater(AppInfo.Current, GitHubClient),
            PluginPlatform.MacOS => new MacOSUpdater(AppInfo.Current, GitHubClient),
            _ => null
        };

        [Obsolete("Retrieve via DI instead")]
        public static ITimer Timer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsTimer(),
            PluginPlatform.Linux => new LinuxTimer(),
            PluginPlatform.MacOS => new MacOSTimer(),
            _ => new FallbackTimer()
        };
    }

    [UsedImplicitly] // used via autofac module discovery
    public class IVirtualScreenModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (OperatingSystem.IsWindows())
            {
                builder.Register(_ => new WindowsDisplay()).As<IVirtualScreen>().InstancePerLifetimeScope();
            }
            else if (OperatingSystem.IsMacOS())
            {
                builder.Register(_ => new MacOSDisplay()).As<IVirtualScreen>().InstancePerLifetimeScope();
            }
            else if (OperatingSystem.IsLinux())
            {
                if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                    builder.Register(_ => new WaylandDisplay()).As<IVirtualScreen>().InstancePerLifetimeScope();
                else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                    builder.Register(_ => new XScreen()).As<IVirtualScreen>().InstancePerLifetimeScope();
            }
        }
    }
}
