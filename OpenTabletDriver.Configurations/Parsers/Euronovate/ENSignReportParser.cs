using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Euronovate
{
    public class ENSignReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x06 when report[1].IsBitSet(4) => new ENSignTabletReport(report),
                0x06 => new OutOfRangeReport(report),
                _ => new DeviceReport(report)
            };
        }
    }
}
