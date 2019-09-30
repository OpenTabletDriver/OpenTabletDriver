using System;
using System.Runtime.InteropServices;

namespace TabletDriverLib.Tools.Native
{
    using XPointer = System.IntPtr;
    using Display = System.IntPtr;
    using Window = System.IntPtr;
    using Drawable = System.IntPtr;
    using Font = System.IntPtr;
    using Pixmap = System.IntPtr;
    using Cursor = System.IntPtr;
    using Colormap = System.IntPtr;
    using GContext = System.IntPtr;
    using KeySym = System.IntPtr;
    using Mask = System.IntPtr;
    using Atom = System.IntPtr;
    using VisualID = System.IntPtr;
    using Time = System.IntPtr;
    
    internal class Linux
    {
        private static object Lock = new object();

        [DllImport("libX11", EntryPoint = "XOpenDisplay")]
        private extern static IntPtr sys_XOpenDisplay(IntPtr display);
        public static Display XOpenDisplay(IntPtr display)
        {
            lock (Lock)
                return sys_XOpenDisplay(display);
        }
        
        [DllImport("libX11", EntryPoint = "XWarpPointer")]
        public extern static uint XWarpPointer(Display display, Window src_w, Window dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);
        
        [DllImport("libX11", EntryPoint = "XFlush")]
        public extern static int XFlush(Display display); 
    }
}