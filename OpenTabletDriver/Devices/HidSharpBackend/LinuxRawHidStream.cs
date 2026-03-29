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
        private readonly FileStream _stream;
        private readonly LinuxRawHidDescriptorInfo _descriptorInfo;
        private readonly Func<int, nuint, IntPtr, int> _ioctl;

        public LinuxRawHidStream(string devicePath, LinuxRawHidDescriptorInfo descriptorInfo)
            : this(CreateStream(devicePath), descriptorInfo, LinuxHidrawInterop.ioctl)
        {
        }

        internal LinuxRawHidStream(string devicePath, LinuxRawHidDescriptorInfo descriptorInfo, Func<int, nuint, IntPtr, int> ioctlHandler)
            : this(CreateStream(devicePath), descriptorInfo, ioctlHandler)
        {
        }

        private LinuxRawHidStream(FileStream stream, LinuxRawHidDescriptorInfo descriptorInfo, Func<int, nuint, IntPtr, int> ioctlHandler)
        {
            _stream = stream;
            _descriptorInfo = descriptorInfo;
            _ioctl = ioctlHandler;
        }

        public byte[] Read()
        {
            if (_descriptorInfo.ReportsUseID)
            {
                var buffer = new byte[_descriptorInfo.InputReportLength];
                var count = _stream.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                    Array.Resize(ref buffer, count);

                return buffer;
            }

            var payloadLength = Math.Max(0, _descriptorInfo.InputReportLength - 1);
            var payload = new byte[payloadLength];
            var countRead = _stream.Read(payload, 0, payload.Length);
            var bufferWithSyntheticReportId = new byte[countRead + 1];
            Array.Copy(payload, 0, bufferWithSyntheticReportId, 1, countRead);
            return bufferWithSyntheticReportId;
        }

        public void Write(byte[] buffer) => _stream.Write(buffer, 0, buffer.Length);

        public unsafe void SetFeature(byte[] buffer)
        {
            fixed (byte* ptr = buffer)
            {
                if (_ioctl(GetFd(), LinuxHidrawInterop.HIDIOCSFEATURE(buffer.Length), (IntPtr)ptr) < 0)
                    throw new IOException("SetFeature failed.");
            }
        }

        public unsafe void GetFeature(byte[] buffer)
        {
            if (!_descriptorInfo.ReportsUseID && buffer.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(buffer), "Unnumbered feature reports require a synthetic report ID slot.");

            fixed (byte* ptr = buffer)
            {
                var requestLength = _descriptorInfo.ReportsUseID ? buffer.Length : buffer.Length - 1;
                var requestBuffer = _descriptorInfo.ReportsUseID ? (IntPtr)ptr : (IntPtr)(ptr + 1);

                if (!_descriptorInfo.ReportsUseID)
                    buffer[1] = buffer[0];

                int result = _ioctl(GetFd(), LinuxHidrawInterop.HIDIOCGFEATURE(requestLength), requestBuffer);
                if (result < 0)
                    throw new IOException("GetFeature failed.");

                var clearOffset = _descriptorInfo.ReportsUseID ? result : result + 1;
                if (clearOffset < buffer.Length)
                    Array.Clear(buffer, clearOffset, buffer.Length - clearOffset);
            }
        }

        private int GetFd() => _stream.SafeFileHandle.DangerousGetHandle().ToInt32();

        public void Dispose() => _stream.Dispose();

        private static FileStream CreateStream(string devicePath) =>
            new FileStream(devicePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
    }
}
