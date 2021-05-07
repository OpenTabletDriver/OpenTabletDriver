using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver
    {
        public DesktopDriver()
        {
            TabletHandler.GetReportParser = GetReportParser;
        }

        private static IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            var pluginRef = AppInfo.PluginManager.GetPluginReference(identifier.ReportParser);
            return pluginRef.Construct<IReportParser<IDeviceReport>>();
        }
    }
}
