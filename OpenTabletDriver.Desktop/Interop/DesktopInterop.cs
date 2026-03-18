using System;
using System.Diagnostics;
using Octokit;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Exotic;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
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

        public static IUpdater Updater => CurrentPlatform switch
        {
            PluginPlatform.Windows => field ??= new WindowsUpdater(AppInfo.Current, GitHubClient),
            PluginPlatform.MacOS => field ??= new MacOSUpdater(AppInfo.Current, GitHubClient),
            _ => null
        };

        public static ITimer Timer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsTimer(),
            PluginPlatform.Linux => new LinuxTimer(),
            PluginPlatform.MacOS => new MacOSTimer(),
            _ => new FallbackTimer()
        };

        public static IAbsolutePointer AbsolutePointer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsAbsolutePointer(),
            PluginPlatform.Linux => field ??= new EvdevAbsolutePointer(),
            PluginPlatform.MacOS => new MacOSAbsolutePointer(),
            _ => null
        };

        public static IRelativePointer RelativePointer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsRelativePointer(),
            PluginPlatform.Linux => field ??= new EvdevRelativePointer(),
            PluginPlatform.MacOS => new MacOSRelativePointer(),
            _ => null
        };

        public static IPressureHandler VirtualTablet => CurrentPlatform switch
        {
            PluginPlatform.Linux => field ??= new EvdevVirtualTablet(),
            _ => null
        };

        public static IVirtualKeyboard VirtualKeyboard => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsVirtualKeyboard(),
            PluginPlatform.Linux => field ??= new EvdevVirtualKeyboard(),
            PluginPlatform.MacOS => field ??= new MacOSVirtualKeyboard(),
            _ => null
        };

        public static IVirtualPad VirtualPad => CurrentPlatform switch
        {
            PluginPlatform.Linux => field ??= new EvdevVirtualPad(),
            _ => null
        };

        public static IVirtualScreen VirtualScreen => field ??= CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsDisplay(),
            PluginPlatform.Linux => ConstructLinuxDisplay(),
            PluginPlatform.MacOS => new MacOSDisplay(),
            _ => null
        };

        private static IVirtualScreen ConstructLinuxDisplay()
        {
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                return new WaylandDisplay();
            else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                return new XScreen();

            Log.Write("Display", "Neither Wayland nor X11 were detected, defaulting to X11.", LogLevel.Warning);
            return new XScreen();
        }
    }
}
