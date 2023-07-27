using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public class TenMoonReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return new TenMoonTabletReport(report);
        }
    }
}
