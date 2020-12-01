using System;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Interop
{
    public static class Platform
    {
        public static IAbsolutePointer VirtualTablet => _virtualTablet.Value;
        public static IRelativePointer VirtualMouse => _virtualMouse.Value;

        public static IVirtualKeyboard KeyboardHandler => _keyboardHandler.Value;
        
        public static IVirtualScreen VirtualScreen => _virtualScreen.Value;
        
        public static ITimer Timer => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => new WindowsTimer(),
            RuntimePlatform.Linux   => new LinuxTimer(),
            _                       => new FallbackTimer()
        };

        private static Lazy<IAbsolutePointer> _virtualTablet = new Lazy<IAbsolutePointer>(() =>
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsAbsolutePointer(),
                RuntimePlatform.Linux   => new EvdevAbsolutePointer(),
                RuntimePlatform.MacOS   => new MacOSAbsolutePointer(),
                _                       => null
            };
        });

        private static Lazy<IRelativePointer> _virtualMouse = new Lazy<IRelativePointer>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsRelativePointer(),
                RuntimePlatform.Linux   => new EvdevRelativePointer(),
                RuntimePlatform.MacOS   => new MacOSRelativePointer(),
                _                       => null
            };
        });

        private static Lazy<IVirtualKeyboard> _keyboardHandler = new Lazy<IVirtualKeyboard>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsVirtualKeyboard(),
                RuntimePlatform.Linux   => new EvdevVirtualKeyboard(),
                RuntimePlatform.MacOS   => new MacOSVirtualKeyboard(),
                _                       => null
            };
        });

        private static Lazy<IVirtualScreen> _virtualScreen = new Lazy<IVirtualScreen>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsDisplay(),
                RuntimePlatform.Linux   => GetLinuxScreen(),
                RuntimePlatform.MacOS   => new MacOSDisplay(),
                _                       => null
            };
        });

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
