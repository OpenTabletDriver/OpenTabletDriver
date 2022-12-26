using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SigEvThread
    {
        public IntPtr function;
        public IntPtr attribute;
    }
}
