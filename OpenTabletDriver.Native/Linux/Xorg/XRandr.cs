using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#nullable enable

namespace OpenTabletDriver.Native.Linux.Xorg
{
    public static partial class XRandr
    {
        private const string libXRandr = "libXrandr.so.2";

        [LibraryImport(libXRandr, EntryPoint = "XRRGetMonitors")]
        [return: MarshalUsing(CountElementName = "nmonitors")]
        public static partial XRRMonitorInfo[] XRRGetMonitors(XLib.XLibDisplayHandle dpy, XLib.XLibWindowHandle window, [MarshalAs(UnmanagedType.Bool)] bool get_active, out int nmonitors);
    }
}
