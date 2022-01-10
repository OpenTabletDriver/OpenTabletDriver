using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfoEx
    {
        public uint size;
        public Rect monitor;
        public Rect work;
        public uint flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string deviceName;
    }
}
