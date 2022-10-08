using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Components
{
    /// <summary>
    /// Provides an enumeration of all supported device hubs.
    /// </summary>
    [PublicAPI]
    public interface IDeviceHubsProvider
    {
        /// <summary>
        /// Enumeration of all supported device hubs.
        /// </summary>
        IEnumerable<IDeviceHub> DeviceHubs { get; }
    }
}
