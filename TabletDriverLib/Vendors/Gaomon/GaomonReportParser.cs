using TabletDriverLib.Tablet;

namespace TabletDriverLib.Vendors.Gaomon
{
    public class GaomonReportParser : IDeviceReportParser
    {
        public IDeviceReport Parse(byte[] data)
        {
            var isAuxReport = ((data[1] & (1 << 5)) != 0) & ((data[1] & (1 << 6)) != 0);
            if (isAuxReport)
                return new GaomonAuxReport(data);
            else
                return new TabletReport(data);
        }
    }
}