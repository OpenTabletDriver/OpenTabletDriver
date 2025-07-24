using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public class WacomDriverIntuosProReportParser : IntuosProReportParser
    {
        public override IDeviceReport Parse(byte[] data)
        {
            return base.Parse(data[1..^0]);
        }
    }
}
