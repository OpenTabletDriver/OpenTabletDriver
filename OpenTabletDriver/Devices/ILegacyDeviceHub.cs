using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Devices
{
    public interface ILegacyDeviceHub
    {
        public bool TryGetDevice(string path, out IDeviceEndpoint endpoint);

        public bool CanEnumeratePorts { get; }

        public IEnumerable<Uri> EnumeratePorts();
    }
}
