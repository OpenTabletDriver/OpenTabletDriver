using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface ICompositeDeviceHub : IDeviceHub
    {
        IEnumerable<IDeviceHub> GetDeviceHubs();
        void ConnectDeviceHub<T>() where T : IDeviceHub;
        void ConnectDeviceHub(IDeviceHub instance);
        void DisconnectDeviceHub<T>() where T : IDeviceHub;
        void DisconnectDeviceHub(IDeviceHub instance);
    }
}