using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Components
{
    /// <summary>
    /// A device hub consisting of one or more device hubs.
    /// </summary>
    [PublicAPI]
    public interface ICompositeDeviceHub : IDeviceHub
    {
        /// <summary>
        /// The device hubs contained in the composite hub.
        /// </summary>
        IEnumerable<IDeviceHub> DeviceHubs { get; }

        /// <summary>
        /// Connects a device hub.
        /// </summary>
        /// <typeparam name="T">The device hub type.</typeparam>
        void ConnectDeviceHub<T>() where T : IDeviceHub;

        /// <summary>
        /// Connects a device hub.
        /// </summary>
        /// <param name="instance">The device hub instance.</param>
        void ConnectDeviceHub(IDeviceHub instance);

        /// <summary>
        /// Disconnects a device hub.
        /// </summary>
        /// <typeparam name="T">The device hub type.</typeparam>
        void DisconnectDeviceHub<T>() where T : IDeviceHub;

        /// <summary>
        /// Disconnects a device hub.
        /// </summary>
        /// <param name="instance">The device hub instance.</param>
        void DisconnectDeviceHub(IDeviceHub instance);
    }
}
