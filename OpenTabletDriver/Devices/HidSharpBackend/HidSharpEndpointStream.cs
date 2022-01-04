using System.IO;
using HidSharp;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public class HidSharpEndpointStream : IDeviceEndpointStream
    {
        internal HidSharpEndpointStream(HidStream stream)
        {
            this.stream = stream;
            stream.ReadTimeout = int.MaxValue;
        }

        private HidStream stream;

        public byte[] Read() => stream.Read();
        public void Write(byte[] buffer) => stream.Write(buffer);

        public void GetFeature(byte[] buffer) => stream.GetFeature(buffer);
        public void SetFeature(byte[] buffer) => stream.SetFeature(buffer);

        public void Dispose() => stream.Dispose();
    }
}
