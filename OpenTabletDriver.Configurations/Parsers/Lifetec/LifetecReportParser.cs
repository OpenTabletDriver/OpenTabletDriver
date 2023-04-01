using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Lifetec
{
    public class LifetecReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report.Length >= 8 && report[0] == 0x02
                ? new LifetecTabletReport(report)
                : new DeviceReport(report);
        }
    }
}
