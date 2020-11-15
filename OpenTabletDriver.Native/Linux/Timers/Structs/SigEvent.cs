using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    public delegate void TimerCallback(SigVal a);

    [StructLayout(LayoutKind.Explicit)]
    public struct SigVal
    {
        [FieldOffset(0)]
        public int sival_int;

        [FieldOffset(0)]
        public IntPtr sival_ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SigEvThread
    {
        public IntPtr function;
        public IntPtr attribute;
    }

    [StructLayout(LayoutKind.Explicit, Size = 64, Pack = 1)]
    public struct SigEvent
    {
        [FieldOffset(0)]
        public SigVal value;

        [FieldOffset(8)]
        public int signo;

        [FieldOffset(12)]
        public SigEv notify;

        [FieldOffset(16)]
        public int tid;

        [FieldOffset(16)]
        public SigEvThread thread;
    }
}