using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using OpenTabletDriver.Components;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations
{
    [PublicAPI]
    public partial class DeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public bool RaisesTabletConfigurationsChanged => false;

#pragma warning disable CS0067 // The event 'DeviceConfigurationProvider.TabletConfigurationsChanged' is never used
        public event Action<ImmutableArray<TabletConfiguration>>? TabletConfigurationsChanged;
#pragma warning restore CS0067
    }
}
