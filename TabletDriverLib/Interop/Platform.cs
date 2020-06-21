using System;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Interop.Keyboard;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;
using TabletDriverPlugin.Platform.Keyboard;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop
{
    public static class Platform
    {
        public static ICursorHandler CursorHandler => _cursorHandler.Value;
        public static IKeyboardHandler KeyboardHandler => _keyboardHandler.Value;
        public static IVirtualScreen VirtualScreen => _virtualScreen.Value;

        private static Lazy<ICursorHandler> _cursorHandler = new Lazy<ICursorHandler>(() =>
        {
            if (PlatformInfo.IsWindows)
                return new WindowsCursorHandler();
            else if (PlatformInfo.IsLinux)
                return new EvdevCursorHandler();
            else if (PlatformInfo.IsOSX)
                return new MacOSCursorHandler();
            else
                return null;
        });

        private static Lazy<IKeyboardHandler> _keyboardHandler = new Lazy<IKeyboardHandler>(() => 
        {
            if (PlatformInfo.IsWindows)
                return new WindowsKeyboardHandler();
            else if (PlatformInfo.IsLinux)
                return new EvdevKeyboardHandler();
            else if (PlatformInfo.IsOSX)
                return new MacOSKeyboardHandler();
            else
                return null;
        });

        private static Lazy<IVirtualScreen> _virtualScreen = new Lazy<IVirtualScreen>(() => 
        {
            if (PlatformInfo.IsWindows)
                return new WindowsDisplay();
            else if (PlatformInfo.IsLinux)
                return new XScreen();
            else if (PlatformInfo.IsOSX)
                return new MacOSDisplay();
            else
                return null;
        });
    }
}