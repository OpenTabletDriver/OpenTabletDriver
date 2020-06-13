using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Vikoo
{
    public class HK708ReportParser : IDeviceReportParser
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new HK708Report(data);
        }
    }
}