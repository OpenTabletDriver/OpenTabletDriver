using System.Linq;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Vendors
{
    public class SkipByteTabletReportParser : TabletReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data.Skip(1).ToArray());
        }
    }
}