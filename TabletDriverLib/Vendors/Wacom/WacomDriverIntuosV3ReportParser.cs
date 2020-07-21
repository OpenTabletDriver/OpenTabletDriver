using System.Linq;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class WacomDriverIntuosV3ReportParser : IntuosV3ReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data[1..^0]);
        }
    }
}