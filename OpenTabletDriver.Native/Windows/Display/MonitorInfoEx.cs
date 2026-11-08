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
        // deviceName is defined as
        //   CHAR szDevice[CCHDEVICENAME];
        // and CCHDEVICENAME is a const that is 32
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string deviceName;
    }
}
