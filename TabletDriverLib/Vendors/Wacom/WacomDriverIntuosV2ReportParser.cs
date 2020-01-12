using System.Linq;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class WacomDriverIntuosV2ReportParser : IntuosV2ReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data.Skip(1).ToArray());
        }
    }
}