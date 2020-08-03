using TabletDriverLib.Tablet;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class IntuosV3ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            if (data[0] == 0x11)
                return new IntuosV3AuxReport(data);
            else
                return new IntuosV3Report(data);
        }
    }
}