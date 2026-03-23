using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Xorg
{
    using Display = IntPtr;
    using IntPtr = IntPtr;
    using Window = IntPtr;

    public class XLib
    {
        private const string libX11 = "libX11.so.6";
        private static object Lock = new object();

        [DllImport(libX11, EntryPoint = "XOpenDisplay")]
        private extern unsafe static IntPtr sys_XOpenDisplay(char* display);
        public static unsafe IntPtr XOpenDisplay(char* display)
        {
            lock (Lock)
                return sys_XOpenDisplay(display);
        }

        [DllImport(libX11, EntryPoint = "XCloseDisplay")]
        public extern static int XCloseDisplay(IntPtr display);

        [DllImport(libX11, EntryPoint = "XDefaultRootWindow")]
        public static extern Window XDefaultRootWindow(Display display);

        [DllImport(libX11, EntryPoint = "XDisplayWidth")]
        public static extern int XDisplayWidth(Display display, int screen_number);

        [DllImport(libX11, EntryPoint = "XDisplayHeight")]
        public static extern int XDisplayHeight(Display display, int screen_number);
    }
}
