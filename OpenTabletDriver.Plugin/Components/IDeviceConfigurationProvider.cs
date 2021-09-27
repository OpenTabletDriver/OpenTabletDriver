using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Components
{
    public interface IDeviceConfigurationProvider
    {
        IEnumerable<TabletConfiguration> TabletConfigurations { get; }
    }
}