using System;
using System.Collections.Immutable;
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
        /// Returns true if the provider is capable of raising the <see cref="TabletConfigurationsChanged"/> event,
        /// otherwise false if the provided configurations are static.
        /// </summary>
        bool RaisesTabletConfigurationsChanged { get; }

        /// <summary>
        /// Enumeration of the configurations for all supported tablets.
        /// </summary>
        ImmutableArray<TabletConfiguration> TabletConfigurations { get; }

        /// <summary>
        /// Occurs when the tablet configurations have changed.
        /// </summary>
        event Action<ImmutableArray<TabletConfiguration>> TabletConfigurationsChanged;
    }
}
