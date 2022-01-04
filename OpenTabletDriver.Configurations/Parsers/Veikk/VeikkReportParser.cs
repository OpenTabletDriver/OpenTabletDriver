using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[2].IsBitSet(5))
                return new VeikkTabletReport(report);

            return new DeviceReport(report);
        }
    }
}
