using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkV1ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[0] == 0x03)
                return new VeikkAuxV1Report(report);

            return (report[1], report[2]) switch
            {
                (0x41, byte x) when (x & 0xF0) == 0xA0 => new TabletReport(report[1..]),
                (0x41, 0xC0) => new OutOfRangeReport(report),
                _ => new DeviceReport(report)
            };
        }
    }
}
