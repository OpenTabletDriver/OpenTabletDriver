using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Debugging
{
    public class DebugDeviceReport : IDeviceReport
    {
        public DebugDeviceReport()
        {
        }

        public DebugDeviceReport(IDeviceReport deviceReport)
        {
            this.Raw = deviceReport.Raw;
            this.Formatted = deviceReport.GetStringFormat();
        }

        public byte[] Raw { set; get; }
        public string Formatted { set; get; }
        public string GetStringFormat() => Formatted;
    }
}