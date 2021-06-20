using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom.Intuos
{
    public class WacomDriverIntuosReportParser : IntuosReportParser
    {
        public override IDeviceReport Parse(byte[] report)
        {
            return base.Parse(report[1..^0]);
        }
    }
}