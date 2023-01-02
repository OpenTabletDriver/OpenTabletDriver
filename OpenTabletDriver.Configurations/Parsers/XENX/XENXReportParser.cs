using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XENX
{
    public class XENXReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x01 => report[1] != 0 ? new XENXTabletReport(report) : new OutOfRangeReport(report),
                0x02 => new XENXAuxReport(report),
                _ => new DeviceReport(report),
            };
        }
    }
}
