using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Daemon.Contracts.Json.Converters.Implementations
{
    internal sealed class SerializableDeviceEndpointStream : Serializable, IDeviceEndpointStream
    {
        public void Dispose() => throw NotSupported();
        public byte[] Read() => throw NotSupported();
        public void Write(byte[] buffer) => throw NotSupported();

        public void GetFeature(byte[] buffer) => throw NotSupported();
        public void SetFeature(byte[] buffer) => throw NotSupported();
    }
}
