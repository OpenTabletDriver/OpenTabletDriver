using System;
using System.Diagnostics;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
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

        private static IUpdater updater;
        private static IVirtualScreen virtualScreen;
        private static IAbsolutePointer absolutePointer;
        private static IRelativePointer relativePointer;
        private static IPressureHandler virtualTablet;
        private static IVirtualKeyboard virtualKeyboard;

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start {path.Replace("&", "^&")}")
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

        public static IUpdater Updater => CurrentPlatform switch
        {
            PluginPlatform.Windows => updater ??= new WindowsUpdater(),
            PluginPlatform.MacOS => updater ??= new MacOSUpdater(),
            _ => null
        };

        public static ITimer Timer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsTimer(),
            _ => new FallbackTimer()
        };

        public static IAbsolutePointer AbsolutePointer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsAbsolutePointer(),
            PluginPlatform.Linux => absolutePointer ??= new EvdevAbsolutePointer(),
            PluginPlatform.MacOS => new MacOSAbsolutePointer(),
            _ => null
        };

        public static IRelativePointer RelativePointer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsRelativePointer(),
            PluginPlatform.Linux => relativePointer ??= new EvdevRelativePointer(),
            PluginPlatform.MacOS => new MacOSRelativePointer(),
            _ => null
        };

        public static IPressureHandler VirtualTablet => CurrentPlatform switch
        {
            PluginPlatform.Linux => virtualTablet ??= new EvdevVirtualTablet(),
            _ => null
        };

        public static IVirtualKeyboard VirtualKeyboard => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsVirtualKeyboard(),
            PluginPlatform.Linux => virtualKeyboard ??= new EvdevVirtualKeyboard(),
            PluginPlatform.MacOS => new MacOSVirtualKeyboard(),
            _ => null
        };

        public static IVirtualScreen VirtualScreen => virtualScreen ??= CurrentPlatform switch
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
