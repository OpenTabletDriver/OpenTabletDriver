using System;
using System.IO;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface IDeviceEndpointStream : IDisposable
    {
        Stream Stream { get; }

        byte[] Read();
        void Write(byte[] buffer);

        void GetFeature(byte[] buffer);
        void SetFeature(byte[] buffer);
    }
}