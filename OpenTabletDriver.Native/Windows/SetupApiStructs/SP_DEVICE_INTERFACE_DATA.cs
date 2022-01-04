using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.SetupApiStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SP_DEVICE_INTERFACE_DATA
    {
        public int cbSize;
        public Guid interfaceClassGuid;
        public int flags;
        private readonly UIntPtr reserved;

        public static SP_DEVICE_INTERFACE_DATA Create()
        {
            return new SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)) };
        }
    }
}
