using System;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverPlugin;

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
                        _cursorHandler = new XCursorHandler();
                    else if (PlatformInfo.IsOSX)
                        _cursorHandler = new MacOSCursorHandler();
                    else
                    {
                        Log.Write("Cursor Handler", $"Failed to create a cursor handler for this platform ({Environment.OSVersion.Platform}).", true);  
                        return null;
                    }
                }
                return _cursorHandler;
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
    }
}