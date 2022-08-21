using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Devices
{
    /// <summary>
    /// A device hub, providing an enumeration of connected devices.
    /// </summary>
    [PublicAPI]
    public interface IDeviceHub
    {
        /// <summary>
        /// Invoked when device changes occur.
        /// </summary>
        event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        /// <summary>
        /// Requests all connected devices from the hub.
        /// </summary>
        /// <returns>
        /// An enumeration of <see cref="IDeviceEndpoint"/>
        /// </returns>
        IEnumerable<IDeviceEndpoint> GetDevices();
    }
}
