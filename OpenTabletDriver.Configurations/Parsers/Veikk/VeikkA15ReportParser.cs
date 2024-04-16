using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkA15ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0x43) // Touchpad report - not yet supported, so ignored
                return new DeviceReport(report);

            if (report[2].IsBitSet(5))
                return new VeikkA15TabletReport(report);
            else if (report[2] == 1)
                return new VeikkAuxReport(report);

            return new DeviceReport(report);
        }
    }
}
