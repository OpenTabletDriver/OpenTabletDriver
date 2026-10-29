using System;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        public IReportParser GetReportParser(string reportParserName)
        {
            var rv = AppInfo.PluginManager.ConstructObject<IReportParser>(reportParserName);

            return rv ?? throw new ArgumentException("Invalid report parser name: " + reportParserName, nameof(reportParserName));
        }
    }
}
