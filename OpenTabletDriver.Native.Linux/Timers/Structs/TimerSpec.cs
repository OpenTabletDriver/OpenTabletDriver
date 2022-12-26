using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Timers.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerSpec
    {
        public TimeSpec interval;
        public TimeSpec value;
    }
}
