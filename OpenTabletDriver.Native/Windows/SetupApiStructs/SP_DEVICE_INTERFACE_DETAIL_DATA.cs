using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.SetupApiStructs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SP_DEVICE_INTERFACE_DETAIL_DATA
    {
        public int cbSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string DevicePath;

        public static SP_DEVICE_INTERFACE_DETAIL_DATA Create()
        {
            return new SP_DEVICE_INTERFACE_DETAIL_DATA { cbSize = IntPtr.Size == 8 ? 8 : 6 };
        }
    }
}
