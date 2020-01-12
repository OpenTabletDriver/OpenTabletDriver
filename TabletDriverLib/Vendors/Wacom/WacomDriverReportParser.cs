using System.Linq;
using TabletDriverLib.Tablet;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class WacomDriverReportParser : TabletReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data.Skip(1).ToArray());
        }
    }
}