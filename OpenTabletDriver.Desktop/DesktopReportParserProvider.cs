using System;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            var rv = AppInfo.PluginManager.ConstructObject<IReportParser<IDeviceReport>>(reportParserName);

            return rv ?? throw new ArgumentException("Invalid report parser name: " + reportParserName, nameof(reportParserName));
        }
    }
}
