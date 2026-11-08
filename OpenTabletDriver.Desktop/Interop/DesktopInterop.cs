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
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop
{
    public class DesktopInterop
    {
        protected DesktopInterop()
        {
        }

        private static IUpdater? updater;
        private static IVirtualScreen? virtualScreen;
        private static IAbsolutePointer? absolutePointer;
        private static IRelativePointer? relativePointer;
        private static IPressureHandler? virtualTablet;
        private static IVirtualKeyboard? virtualKeyboard;
        private static IVirtualPad? virtualPad;

        public static void Open(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                var startInfo = new ProcessStartInfo("cmd", $"/c start explorer \"{path.Replace("&", "^&")}\"")
                {
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", $"\"{path}\"");
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                Process.Start("open", $"\"{path}\"");
            }
        }

        public static void OpenFolder(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer", $"\"{path.Replace("&", "^&")}\"");
            }
            else
            {
                Open(path);
            }
        }

        public static IGitHubClient GitHubClient => new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

        public static IUpdater? Updater => true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsUpdater(AppInfo.Current, GitHubClient),
            _ when OperatingSystem.IsMacOS() => new MacOSUpdater(AppInfo.Current, GitHubClient),
            _ => null
        };

        public static ITimer Timer => true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsTimer(),
            _ when OperatingSystem.IsLinux() => new LinuxTimer(),
            _ when OperatingSystem.IsMacOS() => new MacOSTimer(),
            _ => new FallbackTimer()
        };

        public static IAbsolutePointer? AbsolutePointer => true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsAbsolutePointer(),
            _ when OperatingSystem.IsLinux() => absolutePointer ??= new EvdevAbsolutePointer(),
            _ when OperatingSystem.IsMacOS() => new MacOSAbsolutePointer(),
            _ => null
        };

        public static IRelativePointer? RelativePointer => true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsRelativePointer(),
            _ when OperatingSystem.IsLinux() => relativePointer ??= new EvdevRelativePointer(),
            _ when OperatingSystem.IsMacOS() => new MacOSRelativePointer(),
            _ => null
        };

        public static IPressureHandler? VirtualTablet => true switch
        {
            _ when OperatingSystem.IsLinux() => virtualTablet ??= new EvdevVirtualTablet(),
            _ => null
        };

        public static IVirtualKeyboard? VirtualKeyboard => true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsVirtualKeyboard(),
            _ when OperatingSystem.IsLinux() => virtualKeyboard ??= new EvdevVirtualKeyboard(),
            _ when OperatingSystem.IsMacOS() => virtualKeyboard ??= new MacOSVirtualKeyboard(),
            _ => null
        };

        public static IVirtualPad? VirtualPad => true switch
        {
            _ when OperatingSystem.IsLinux() => virtualPad ??= new EvdevVirtualPad(),
            _ => null
        };

        public static IVirtualScreen? VirtualScreen => virtualScreen ??= true switch
        {
            _ when OperatingSystem.IsWindows() => new WindowsDisplay(),
            _ when OperatingSystem.IsLinux() => ConstructLinuxDisplay(),
            _ when OperatingSystem.IsMacOS() => new MacOSDisplay(),
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
