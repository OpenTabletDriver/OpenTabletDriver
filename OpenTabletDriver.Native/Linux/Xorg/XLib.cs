using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

#nullable enable

namespace OpenTabletDriver.Native.Linux.Xorg
{
    public partial class XLib
    {
        public abstract class XLibHandle() : SafeHandleZeroOrMinusOneIsInvalid(true)
        {
            protected override bool ReleaseHandle() => XCloseDisplay(handle) == 0;
        }

        public class XLibDisplayHandle : XLibHandle;
        public class XLibWindowHandle : XLibHandle
        {
            protected override bool ReleaseHandle() => true; // nothing to clean up
        }

        private const string libX11 = "libX11.so.6";
        private static object Lock = new object();

        [LibraryImport(libX11, EntryPoint = "XOpenDisplay")]
        private static partial XLibDisplayHandle sys_XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string? display);
        public static XLibDisplayHandle XOpenDisplay(string? display)
        {
            lock (Lock)
                return sys_XOpenDisplay(display);
        }

        [LibraryImport(libX11)]
        private static partial int XCloseDisplay(IntPtr display);

        [LibraryImport(libX11)]
        public static partial XLibWindowHandle XDefaultRootWindow(XLibDisplayHandle display);

        [LibraryImport(libX11)]
        public static partial int XDisplayWidth(XLibDisplayHandle display, int screen_number);

        [LibraryImport(libX11)]
        public static partial int XDisplayHeight(XLibDisplayHandle display, int screen_number);
    }
}
