using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop
{
    public static class SystemInterop
    {
        public static IAbsolutePointer VirtualTablet => _virtualTablet.Value;
        public static IRelativePointer VirtualMouse => _virtualMouse.Value;

        public static IVirtualKeyboard KeyboardHandler => _keyboardHandler.Value;
        
        public static IVirtualScreen VirtualScreen => _virtualScreen.Value;
        
        public static ITimer Timer => CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsTimer(),
            PluginPlatform.Linux   => new LinuxTimer(),
            _                       => new FallbackTimer()
        };

        private static Lazy<IAbsolutePointer> _virtualTablet = new Lazy<IAbsolutePointer>(() =>
        {
            return CurrentPlatform switch
            {
                PluginPlatform.Windows => new WindowsAbsolutePointer(),
                PluginPlatform.Linux   => new EvdevAbsolutePointer(),
                PluginPlatform.MacOS   => new MacOSAbsolutePointer(),
                _                       => null
            };
        });

        private static Lazy<IRelativePointer> _virtualMouse = new Lazy<IRelativePointer>(() => 
        {
            return CurrentPlatform switch
            {
                PluginPlatform.Windows => new WindowsRelativePointer(),
                PluginPlatform.Linux   => new EvdevRelativePointer(),
                PluginPlatform.MacOS   => new MacOSRelativePointer(),
                _                       => null
            };
        });

        private static Lazy<IVirtualKeyboard> _keyboardHandler = new Lazy<IVirtualKeyboard>(() => 
        {
            return CurrentPlatform switch
            {
                PluginPlatform.Windows => new WindowsVirtualKeyboard(),
                PluginPlatform.Linux   => new EvdevVirtualKeyboard(),
                PluginPlatform.MacOS   => new MacOSVirtualKeyboard(),
                _                       => null
            };
        });

        private static Lazy<IVirtualScreen> _virtualScreen = new Lazy<IVirtualScreen>(() => 
        {
            return CurrentPlatform switch
            {
                PluginPlatform.Windows => new WindowsDisplay(),
                PluginPlatform.Linux   => GetLinuxScreen(),
                PluginPlatform.MacOS   => new MacOSDisplay(),
                _                       => null
            };
        });

        public static PluginPlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return PluginPlatform.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return PluginPlatform.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return PluginPlatform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return PluginPlatform.FreeBSD;
                else
                    return 0;
            }
        }

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start \"{path.Replace("&", "^&")}\"")
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

        private static IVirtualScreen GetLinuxScreen()
        {
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                return new WaylandDisplay();
            else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                return new XScreen();
            else
                throw new Exception("Neither Wayland nor X11 were detected. Make sure DISPLAY or WAYLAND_DISPLAY is set.");
        }
    }
}
