using System;
using HidSharp;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public sealed class HidSharpEndpointStream : IDeviceEndpointStream
    {
        internal HidSharpEndpointStream(HidStream stream)
        {
            this.stream = stream;
            stream.ReadTimeout = int.MaxValue;
        }

        private HidStream stream;

        // TODO: implement Span<byte> handling directly in HIDSharp to avoid a copy
        public int Read(Span<byte> buffer) => stream.Read(buffer);
        public void Write(byte[] buffer) => stream.Write(buffer);

        public void GetFeature(byte[] buffer) => stream.GetFeature(buffer);
        public void SetFeature(byte[] buffer) => stream.SetFeature(buffer);

        public void Dispose() => stream.Dispose();
    }
}
