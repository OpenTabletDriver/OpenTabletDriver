using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimeSpec
    {
        public long sec;
        public long nsec;
    }
}
