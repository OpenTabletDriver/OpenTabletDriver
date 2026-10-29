using System;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface IDeviceEndpointStream : IDisposable
    {
        int Read(Span<byte> buffer);
        void Write(byte[] buffer);

        void GetFeature(byte[] buffer);
        void SetFeature(byte[] buffer);
    }
}
