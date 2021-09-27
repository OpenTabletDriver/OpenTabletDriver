using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public IEnumerable<TabletConfiguration> TabletConfigurations
        {
            get
            {
                var files = Directory.EnumerateFiles(AppInfo.Current.ConfigurationDirectory, "*.json", SearchOption.AllDirectories);
                return files.Select(path => Serialization.Deserialize<TabletConfiguration>(File.OpenRead(path)));
            }
        }
    }
}