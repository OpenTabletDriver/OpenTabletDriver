using System;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface IDeviceEndpointStream : IDisposable
    {
        byte[] Read();
        void Write(byte[] buffer);

        void GetFeature(byte[] buffer);
        void SetFeature(byte[] buffer);
    }
}
