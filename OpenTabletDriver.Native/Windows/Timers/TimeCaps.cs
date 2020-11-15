using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.Timers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TimeCaps
    {
        public uint wPeriodMin;
        public uint wPeriodMax;
    };
}