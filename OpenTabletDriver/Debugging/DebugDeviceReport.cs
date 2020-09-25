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
        }

        public byte[] Raw { set; get; }
    }
}