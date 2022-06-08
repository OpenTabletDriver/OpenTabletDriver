using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[2].IsBitSet(5))
                return new VeikkTabletReport(report);
            else if (report[1] == 0x42)
                return new VeikkAuxReport(report);
            else if (report[1] == 0x43)
                return new VeikkGestureTouchpadReport(report);

            return new DeviceReport(report);
        }
    }
}
