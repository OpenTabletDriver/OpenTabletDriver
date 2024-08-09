using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ITimerSpec
    {
        public TimeSpec it_interval;
        public TimeSpec it_value;
    }
}
