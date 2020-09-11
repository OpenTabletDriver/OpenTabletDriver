using System;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public static class DriverState
    {
        public static IOutputMode OutputMode { set; get; }
        public static TabletProperties TabletProperties { set; get; }
        public static event EventHandler<IDeviceReport> ReportRecieved;

        public static void PostReport(object sender, IDeviceReport report)
        {
            ReportRecieved?.Invoke(sender, report);
        }
    }
}