using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Components
{
    /// <summary>
    /// Provides configurations for all supported tablets.
    /// </summary>
    [PublicAPI]
    public interface IDeviceConfigurationProvider
    {
        /// <summary>
        /// Enumeration of the configurations for all supported tablets.
        /// </summary>
        IEnumerable<TabletConfiguration> TabletConfigurations { get; }
    }
}
