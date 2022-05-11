using System.Collections.Generic;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Plugin.Components
{
    public interface ICompositeDeviceHub : IDeviceHub
    {
        IEnumerable<IDeviceHub> DeviceHubs { get; }

        IEnumerable<ILegacyDeviceHub> LegacyDeviceHubs { get; }

        IEnumerable<string> LegacyPortNames { get; }

        void ConnectDeviceHub<T>() where T : IDeviceHub;
        void ConnectDeviceHub(IDeviceHub instance);
        void DisconnectDeviceHub<T>() where T : IDeviceHub;
        void DisconnectDeviceHub(IDeviceHub instance);
    }
}
