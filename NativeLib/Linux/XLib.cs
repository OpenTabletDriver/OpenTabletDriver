using System;
using System.Runtime.InteropServices;

namespace NativeLib.Linux
{
    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;
    
    public class XLib
    {
        private const string libX11 = "libX11";
        private const string libXtst = "libXtst";
        private static object Lock = new object();
        
        [DllImport(libX11, EntryPoint = "XQueryPointer")]
        public extern static bool XQueryPointer(Display display, Window window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

        [DllImport(libX11, EntryPoint = "XWarpPointer")]
        public extern static uint XWarpPointer(Display display, Window src_w, Window dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

        [DllImport(libX11, EntryPoint = "XSendEvent")]
        public static extern int XSendEvent(Display display, Window window, bool propagate, long event_mask, IntPtr event_send);

        [DllImport(libXtst, EntryPoint = "XTestFakeButtonEvent")]
        public static extern int XTestFakeButtonEvent(Display display, Button button, bool is_press, ulong delay);

        [DllImport(libXtst, EntryPoint = "XTestFakeKeyEvent")]
        public static extern int XTestFakeKeyEvent(Display display, uint keycode, bool is_press, ulong delay);

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
        public extern static int XFlush(Display display); 

        [DllImport(libX11, EntryPoint = "XDefaultRootWindow")]
        public static extern Window XDefaultRootWindow(Display display);

        [DllImport(libX11, EntryPoint = "XDisplayWidth")]
        public static extern int XDisplayWidth(Display display, int screen_number);

        [DllImport(libX11, EntryPoint = "XDisplayHeight")]
        public static extern int XDisplayHeight(Display display, int screen_number);
    }
}