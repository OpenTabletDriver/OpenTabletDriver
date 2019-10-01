using System;
using System.Runtime.InteropServices;

namespace TabletDriverLib.Tools.Native
{
    using XPointer = System.IntPtr;
    using Display = System.IntPtr;
    using Window = System.IntPtr;
    
    internal class Linux
    {
        private const string libX11 = "libX11";

        private static object Lock = new object();

        [DllImport(libX11, EntryPoint = "XQueryPointer")]
        public extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

        [DllImport(libX11, EntryPoint = "XWarpPointer")]
        public extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

        #region Display Management

        [DllImport(libX11, EntryPoint = "XOpenDisplay")]
        private extern unsafe static IntPtr sys_XOpenDisplay(char* display);
        public static unsafe IntPtr XOpenDisplay(char* display)
        {
            lock (Lock)
                return sys_XOpenDisplay(display);
        }

        [DllImport(libX11, EntryPoint = "XCloseDisplay")]
        public extern static int XCloseDisplay(IntPtr display);

        [DllImport(libX11, EntryPoint = "XFlush")]
        public extern static int XFlush(IntPtr display); 

        [DllImport(libX11)]
        public static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport(libX11)]
        public static extern int XDisplayWidth(IntPtr display, int screen_number);

        [DllImport(libX11)]
        public static extern int XDisplayHeight(IntPtr display, int screen_number);

        #endregion
    }
}