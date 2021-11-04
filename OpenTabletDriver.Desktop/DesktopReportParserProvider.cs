using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return AppInfo.PluginManager.ConstructObject<IReportParser<IDeviceReport>>(reportParserName);
        }
    }
}