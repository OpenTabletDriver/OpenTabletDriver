using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver.Devices.SerialBackend
{
    [DeviceHub]
    public class SerialDeviceHub : ILegacyDeviceHub
    {
        public SerialDeviceHub()
        {
        }

        public bool CanEnumeratePorts => true;

        public IEnumerable<Uri> EnumeratePorts()
        {
            foreach (string port in SerialPort.GetPortNames())
            {
                yield return new Uri("serial://" + port);
            }
        }

        public bool TryGetDevice(string path, out IDeviceEndpoint endpoint)
        {
            try
            {
                endpoint = new SerialInterface(path);
                return true;
            }
            catch
            {
                endpoint = null;
                return false;
            }
        }
    }
}
