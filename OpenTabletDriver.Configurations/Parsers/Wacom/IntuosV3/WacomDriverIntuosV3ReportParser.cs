using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV3
{
    public class WacomDriverIntuosV3ReportParser : IntuosV3ReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data[1..^0]);
        }
    }
}
