using System;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Interop.Input;

namespace TabletDriverLib.Interop
{
    public static class Platform
    {
        public static IInputHandler InputHandler
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return new WindowsInputHandler();
                    case PlatformID.Unix:
                        return new XInputHandler();
                    case PlatformID.MacOSX:
                        return new MacOSInputHandler();
                    default:
                        Log.Fail($"Failed to create an input handler for this platform ({Environment.OSVersion.Platform}).");
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