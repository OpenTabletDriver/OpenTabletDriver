using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfo
    {
        public uint size;
        public Rect monitor;
        public Rect work;
        public uint flags;
    }
}