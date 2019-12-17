using System;
using NativeLib;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;

namespace TabletDriverLib.Interop
{
    public static class Platform
    {
        public static ICursorHandler CursorHandler
        {
            get
            {
                if (PlatformInfo.IsWindows)
                    return new WindowsCursorHandler();
                else if (PlatformInfo.IsLinux)
                    return new XCursorHandler();
                else if (PlatformInfo.IsOSX)
                    return new MacOSCursorHandler();
                
                Log.Write("Cursor Handler", $"Failed to create a cursor handler for this platform ({Environment.OSVersion.Platform}).", true);
                return null;
            }
        }

        public static IVirtualScreen Display
        {
            get
            {
                if (PlatformInfo.IsWindows)
                    return new WindowsDisplay();
                else if (PlatformInfo.IsLinux)
                    return new XDisplay();
                else if (PlatformInfo.IsOSX)
                    return new MacOSDisplay();
                    
                Log.Write("Display Handler", $"Failed to create a display handler for this platform ({Environment.OSVersion.Platform}).", true);
                return null;
            }
        }
    }
}