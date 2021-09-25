using System.Collections.Generic;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Plugin
{
    public interface IDeviceHubsProvider
    {
        IEnumerable<IDeviceHub> GetDeviceHubs();
    }
}