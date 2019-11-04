using System;
using System.Runtime.InteropServices;

namespace NativeLib.Linux
{
    using Window = IntPtr;
    using Display = IntPtr;

    [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
    public struct XEvent
    {
        public int type;
        public ulong serial;
        public bool send_event;
        public Display display;
        public Window window;
    }
}