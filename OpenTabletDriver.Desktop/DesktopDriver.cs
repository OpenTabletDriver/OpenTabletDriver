using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver
    {
        public override IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            var pluginRef = AppInfo.PluginManager.GetPluginReference(identifier.ReportParser);
            return pluginRef.Construct<IReportParser<IDeviceReport>>();
        }

        protected override IEnumerable<TabletConfiguration> GetTabletConfigurations()
        {
            var files = Directory.EnumerateFiles(AppInfo.Current.ConfigurationDirectory, "*.json", SearchOption.AllDirectories);
            return files.Select(path => Serialization.Deserialize<TabletConfiguration>(File.OpenRead(path)));
        }
    }
}
