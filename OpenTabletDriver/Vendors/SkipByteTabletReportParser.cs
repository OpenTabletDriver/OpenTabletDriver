using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors
{
    public class SkipByteTabletReportParser : TabletReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data[1..^0]);
        }
    }
}