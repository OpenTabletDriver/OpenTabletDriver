using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class WacomTouchReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            lastReport = new WacomTouchReport(data, lastReport);
            return lastReport;
        }
        private WacomTouchReport? lastReport;
    }
}
