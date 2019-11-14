using System;
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
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return new WindowsCursorHandler();
                    case PlatformID.Unix:
                        return new XCursorHandler();
                    case PlatformID.MacOSX:
                        return new MacOSCursorHandler();
                    default:
                        Log.Fail($"Failed to create a cursor handler for this platform ({Environment.OSVersion.Platform}).");
                        return null;
                }
            }
        }

        public static IDisplay Display
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return new WindowsDisplay();
                    case PlatformID.Unix:
                        return new XDisplay();
                    case PlatformID.MacOSX:
                        return new MacOSDisplay();
                    default:
                        Log.Fail($"Failed to create a display handler for this platform ({Environment.OSVersion.Platform}).");
                        return null;
                }
            }
        }
    }
}