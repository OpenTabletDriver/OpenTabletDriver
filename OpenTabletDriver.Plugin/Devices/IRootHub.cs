using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface IRootHub
    {
        event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        IEnumerable<IDeviceEndpoint> GetDevices();        
    }
}