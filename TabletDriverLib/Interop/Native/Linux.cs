using System;
using System.Runtime.InteropServices;

namespace TabletDriverLib.Interop.Native
{
    using IntPtr = IntPtr;
    using Display = IntPtr;
    using Window = IntPtr;
    
    internal class Linux
    {
        private const string libX11 = "libX11";
        private const string libXtst = "libXtst";

        #region Native Structs

        private static object Lock = new object();

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XEvent
        {
            public int type;
            public ulong serial;
            public bool send_event;
            public IntPtr display;
            public Window window;
        }

        [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
        public struct XButtonEvent
        {
            public int type;
            public ulong serial;
            public bool send_event;
            public Display display;
            public Window window;
            public Window root;
            public Window subwindow;
            public ulong time;
            public int x, y;
            public int x_root, y_root;
            public uint state;
            public Button button;
            public bool same_screen;
        }

        #endregion

        #region Enums

        public enum Button : uint
        {
            LEFT = 1,
            MIDDLE = 2,
            RIGHT = 3,
            BACKWARD = 8,
            FORWARD = 9
        }

        public enum EventMask : long
        {
            NoEventMask = 0L,
            KeyPressMask = (1L << 0),
            KeyReleaseMask = (1L << 1),
            ButtonPressMask = (1L << 2),
            ButtonReleaseMask = (1L << 3)
        }

        #endregion

        #region Input

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

        #endregion

        #region Display

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