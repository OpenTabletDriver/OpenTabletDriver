using System;
using NativeLib;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Interop.Keyboard;
using TabletDriverLib.Interop.Mouse;
using TabletDriverPlugin.Platform.Display;
using TabletDriverPlugin.Platform.Keyboard;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop
{
    public static class Platform
    {
        public static IMouseHandler MouseHandler => _mouseHandler.Value;
        public static IKeyboardHandler KeyboardHandler => _keyboardHandler.Value;
        public static IVirtualScreen VirtualScreen => _virtualScreen.Value;

        private static Lazy<IMouseHandler> _mouseHandler = new Lazy<IMouseHandler>(() =>
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsMouseHandler(),
                RuntimePlatform.Linux   => new EvdevMouseHandler(),
                RuntimePlatform.MacOS   => new MacOSMouseHandler(),
                _                       => null
            };
        });

        private static Lazy<IKeyboardHandler> _keyboardHandler = new Lazy<IKeyboardHandler>(() => 
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsKeyboardHandler(),
                RuntimePlatform.Linux   => new EvdevKeyboardHandler(),
                RuntimePlatform.MacOS   => new MacOSKeyboardHandler(),
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