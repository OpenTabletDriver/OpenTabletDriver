using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    /// <summary>
    /// A raw Linux hidraw stream for devices whose HID report descriptors cannot be parsed by HidSharp.
    /// Falls back to direct file I/O since the kernel's hidraw interface exposes raw reports without
    /// requiring the report descriptor to be parseable.
    /// </summary>
    internal sealed class LinuxRawHidStream : IDeviceEndpointStream
    {
        private const int MaxReportSize = 64;
        private readonly FileStream _stream;

        [DllImport("libc", SetLastError = true)]
        private static extern int ioctl(int fd, nuint request, IntPtr data);

        // hidraw ioctl commands: IOWR('H', nr, len) = (3 << 30) | (len << 16) | ('H' << 8) | nr
        private static nuint HIDIOCSFEATURE(int len) => (nuint)((3u << 30) | ((uint)len << 16) | (72u << 8) | 6u);
        private static nuint HIDIOCGFEATURE(int len) => (nuint)((3u << 30) | ((uint)len << 16) | (72u << 8) | 7u);

        public LinuxRawHidStream(string devicePath)
        {
            _stream = new FileStream(devicePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public byte[] Read()
        {
            var buffer = new byte[MaxReportSize];
            var count = _stream.Read(buffer, 0, buffer.Length);
            if (count != buffer.Length)
                Array.Resize(ref buffer, count);
            return buffer;
        }

        public void Write(byte[] buffer) => _stream.Write(buffer, 0, buffer.Length);

        public unsafe void SetFeature(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                if (ioctl(GetFd(), HIDIOCSFEATURE(buffer.Length), (IntPtr)ptr) < 0)
                    throw new IOException("SetFeature failed.");
            }
        }

        public unsafe void GetFeature(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                int result = ioctl(GetFd(), HIDIOCGFEATURE(buffer.Length), (IntPtr)ptr);
                if (result < 0)
                    throw new IOException("GetFeature failed.");
                if (result < buffer.Length)
                    Array.Clear(buffer, result, buffer.Length - result);
            }
        }

        private int GetFd() => _stream.SafeFileHandle.DangerousGetHandle().ToInt32();

        public void Dispose() => _stream.Dispose();
    }
}
