using System;
using TabletDriverPlugin.Output;
using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin
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