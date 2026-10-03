using System;
using System.Diagnostics;
using Autofac;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            Debug.Assert(AppInfo.PluginManager.Container != null,
                "AppInfo.PluginManager.Container expected non-null at this point");
            var rv = AppInfo.PluginManager.Container?.ResolveKeyed<IReportParser<IDeviceReport>>(reportParserName);

            return rv ?? throw new ArgumentException("Invalid report parser name: " + reportParserName, nameof(reportParserName));
        }
    }
}
