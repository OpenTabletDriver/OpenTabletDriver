using System;
using OpenTabletDriver.Interop.Display;
using OpenTabletDriver.Interop.Input.Keyboard;
using OpenTabletDriver.Interop.Input.Mouse;
using OpenTabletDriver.Interop.Input.Tablet;
using OpenTabletDriver.Interop.Timer;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Interop
{
    public static class Platform
    {
        public static IVirtualPointer VirtualPointer => _virtualMouse.IsValueCreated ? (IVirtualPointer)VirtualMouse : VirtualTablet;
        public static IVirtualTablet VirtualTablet => _virtualTablet.Value;
        public static IVirtualMouse VirtualMouse => _virtualMouse.Value;

        public static IVirtualKeyboard KeyboardHandler => _keyboardHandler.Value;
        
        public static IVirtualScreen VirtualScreen => _virtualScreen.Value;
        
        public static ITimer Timer => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => new WindowsTimer(),
            RuntimePlatform.Linux   => new LinuxTimer(),
            _                       => new FallbackTimer()
        };

        private static Lazy<IVirtualTablet> _virtualTablet = new Lazy<IVirtualTablet>(() =>
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsVirtualTablet(),
                RuntimePlatform.Linux   => new EvdevVirtualTablet(),
                RuntimePlatform.MacOS   => new MacOSVirtualTablet(),
                _                       => null
            };
        });

        private static Lazy<IVirtualMouse> _virtualMouse = new Lazy<IVirtualMouse>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsVirtualMouse(),
                RuntimePlatform.Linux   => new EvdevVirtualMouse(),
                RuntimePlatform.MacOS   => new MacOSVirtualMouse(),
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