using System;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Interop.Keyboard;
using TabletDriverLib.Interop.USB;
using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;
using TabletDriverPlugin.Platform.Keyboard;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Interop
{
    public static class Platform
    {
        private static ICursorHandler _cursorHandler;
        public static ICursorHandler CursorHandler
        {
            get
            {
                if (_cursorHandler == null)
                {
                    if (PlatformInfo.IsWindows)
                        _cursorHandler = new WindowsCursorHandler();
                    else if (PlatformInfo.IsLinux)
                        _cursorHandler = new EvdevCursorHandler();
                    else if (PlatformInfo.IsOSX)
                        _cursorHandler = new MacOSCursorHandler();
                    else
                    {
                        Log.Write("CursorHandler", $"Failed to create a cursor handler for this platform ({Environment.OSVersion.Platform}).", true);
                        return null;
                    }
                }
                return _cursorHandler;
            }
        }

        private static IKeyboardHandler _keyboardHandler;
        public static IKeyboardHandler KeyboardHandler
        {
            get
            {
                if (_keyboardHandler == null)
                {
                    if (PlatformInfo.IsWindows)
                        _keyboardHandler = new WindowsKeyboardHandler();
                    else if (PlatformInfo.IsLinux)
                        _keyboardHandler = new EvdevKeyboardHandler();
                    else if (PlatformInfo.IsOSX)
                        _keyboardHandler = new MacOsKeyboardHandler();
                    else
                    {
                        Log.Write("KeyboardHandler", $"Failed to create a keyboard handler for this platform ({Environment.OSVersion.Platform}).", true);
                        return null;
                    }
                }
                return _keyboardHandler;
            }
        }

        public static IVirtualScreen VirtualScreen
        {
            get
            {
                if (PlatformInfo.IsWindows)
                    return new WindowsDisplay();
                else if (PlatformInfo.IsLinux)
                    return new XScreen();
                else if (PlatformInfo.IsOSX)
                    return new MacOSDisplay();

                Log.Write("Display Handler", $"Failed to create a display handler for this platform ({Environment.OSVersion.Platform}).", true);
                return null;
            }
        }

        public static IUSBUtility USBUtility
        {
            get
            {
                if (PlatformInfo.IsWindows)
                    return new WindowsUSBUtility();
                else if (PlatformInfo.IsLinux)
                    return new EvdevUSBUtility();
                else if (PlatformInfo.IsOSX)
                    return new MacoSUSBUtility();

                Log.Write("USBU tility", $"Failed to create usb utility for this platform ({Environment.OSVersion.Platform}).", true);
                return null;
            }
        }
    }
}
