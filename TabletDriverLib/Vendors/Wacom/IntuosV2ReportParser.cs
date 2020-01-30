using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class IntuosV2ReportParser : IDeviceReportParser
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new IntuosV2TabletReport(data);
        }
    }
}