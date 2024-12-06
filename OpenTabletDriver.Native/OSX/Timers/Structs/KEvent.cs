using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX.Timers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KEvent
    {
        public UIntPtr ident;
        public FilterType filter;
        public Flags flags;
        public FilterFlags fflags;
        public IntPtr data;
        public IntPtr udata;
    }
}
