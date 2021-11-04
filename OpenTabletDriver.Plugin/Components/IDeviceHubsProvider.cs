using System.Collections.Generic;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Plugin.Components
{
    public interface IDeviceHubsProvider
    {
        IEnumerable<IDeviceHub> DeviceHubs { get; }
    }
}