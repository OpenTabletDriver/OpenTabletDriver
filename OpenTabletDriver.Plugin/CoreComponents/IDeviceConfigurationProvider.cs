using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDeviceConfigurationProvider
    {
        IEnumerable<TabletConfiguration> GetTabletConfigurations();
    }
}