using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    internal static class LinuxHidrawInterop
    {
        internal const int HidMaxDescriptorSize = 4096;

        [StructLayout(LayoutKind.Sequential)]
        internal struct HidrawReportDescriptor
        {
            public uint Size;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HidMaxDescriptorSize)]
            public byte[] Value;
        }

        [DllImport("libc", SetLastError = true)]
        internal static extern int ioctl(int fd, nuint request, IntPtr data);

        [DllImport("libc", SetLastError = true)]
        internal static extern int ioctl(int fd, nuint request, out int data);

        [DllImport("libc", SetLastError = true)]
        internal static extern int ioctl(int fd, nuint request, ref HidrawReportDescriptor data);

        internal static readonly nuint HIDIOCGRDESCSIZE = IOR(72, 0x01, sizeof(int));
        internal static readonly nuint HIDIOCGRDESC = IOR(72, 0x02, Marshal.SizeOf(typeof(HidrawReportDescriptor)));

        internal static nuint HIDIOCSFEATURE(int len) => IOWR(72, 0x06, len);
        internal static nuint HIDIOCGFEATURE(int len) => IOWR(72, 0x07, len);

        private static nuint IOR(uint type, uint nr, int size) => IOC(2u, type, nr, (uint)size);
        private static nuint IOWR(uint type, uint nr, int size) => IOC(3u, type, nr, (uint)size);

        private static nuint IOC(uint direction, uint type, uint nr, uint size) =>
            (nuint)((direction << 30) | (size << 16) | (type << 8) | nr);
    }
}
