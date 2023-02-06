using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkTiltReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[1] switch
            {
                0x41 when report[2] is 0xC0 => new OutOfRangeReport(report),
                0x41 => new VeikkTiltTabletReport(report),
                0x42 => new VeikkAuxReport(report),
                0x43 => new DeviceReport(report),// touchpad report not supported yet
                _ => new DeviceReport(report),
            };
        }
    }
}

