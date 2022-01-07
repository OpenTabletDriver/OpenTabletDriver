using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
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
