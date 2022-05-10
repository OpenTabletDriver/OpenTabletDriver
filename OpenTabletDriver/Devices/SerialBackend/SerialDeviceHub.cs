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

        public string[] EnumeratePorts() => SerialPort.GetPortNames();

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
