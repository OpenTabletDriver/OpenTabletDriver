using System;
using System.Collections.Immutable;
using OpenTabletDriver.Components;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations
{
    public partial class DeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public bool RaisesTabletConfigurationsChanged => false;
        public event Action<ImmutableArray<TabletConfiguration>>? TabletConfigurationsChanged;
    }
}
