using System;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Interop.Keyboard;
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
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => new WindowsCursorHandler(),
                RuntimePlatform.Linux   => new EvdevCursorHandler(),
                RuntimePlatform.MacOS   => new MacOSCursorHandler(),
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
                RuntimePlatform.Linux   => new XScreen(),
                RuntimePlatform.MacOS   => new MacOSDisplay(),
                _                       => null
            };
        });
    }
}