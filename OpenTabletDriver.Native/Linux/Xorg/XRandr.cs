using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Xorg
{
    using Display = IntPtr;
    using Window = IntPtr;

    public static unsafe partial class XRandr
    {
        private const string libXRandr = "libXrandr.so.2";

        [LibraryImport(libXRandr, EntryPoint = "XRRGetMonitors")]
        private static partial XRRMonitorInfoNative* _XRRGetMonitors(Display dpy, Window window, [MarshalAs(UnmanagedType.I1)] bool get_active, out int nmonitors);

        public static XRRMonitorInfo[] XRRGetMonitors(Display dpy, Window window, bool get_active)
        {
            var xRandrMonitors = _XRRGetMonitors(dpy, window, get_active, out var count);
            var monitors = new XRRMonitorInfo[count];

            for (int i = 0; i < count; i++)
                monitors[i] = xRandrMonitors[i];

            _XRRFreeMonitors(xRandrMonitors);

            return monitors;
        }

        [LibraryImport(libXRandr, EntryPoint = "XRRFreeMonitors")]
        private static partial void _XRRFreeMonitors(XRRMonitorInfoNative* monitors);
    }
}
