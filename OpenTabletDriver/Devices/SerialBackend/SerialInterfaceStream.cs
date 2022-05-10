using System;
using System.IO.Ports;

namespace OpenTabletDriver.Devices.SerialBackend
{
    public unsafe class SerialInterfaceStream : IDeviceEndpointStream
    {
        private readonly WeakReference<SerialInterface> parentInterface;

        private readonly SerialPort port;

        public SerialInterfaceStream(WeakReference<SerialInterface> parentInterface)
        {
            this.parentInterface = parentInterface;

            if (!parentInterface.TryGetTarget(out var serialInterface))
                throw new InvalidOperationException("Weak reference to parent interface is unexpectedly invalid");

            port = new SerialPort(serialInterface.DevicePath);
            port.Open();
        }

        public byte[] Read()
        {
            //NOTE: idk if we should use the internal buffer size instead
            byte[] buf = new byte[port.BytesToRead];
            port.Read(buf, 0, buf.Length);
            return buf;
        }

        public void Write(byte[] buffer)
        {
            port.Write(buffer, 0, buffer.Length);
        }

        public unsafe void GetFeature(byte[] buffer)
        {
        }

        public void SetFeature(byte[] buffer)
        {
        }

        public void Dispose()
        {
            port.Dispose();
        }
    }
}
