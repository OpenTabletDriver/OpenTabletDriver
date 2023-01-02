using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SigVal
    {
        [FieldOffset(0)]
        public int sival_int;

        [FieldOffset(0)]
        public IntPtr sival_ptr;
    }
}
