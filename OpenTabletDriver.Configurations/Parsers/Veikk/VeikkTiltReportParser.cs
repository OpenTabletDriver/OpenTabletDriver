using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkTiltReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0x41) {
                if (report[2] == 0xC0) return new OutOfRangeReport(report);

                return new VeikkTiltTabletReport(report);
            }

            
            if (report[1] == 0x42) return new VeikkAuxReport(report);

            if (report[1] == 0x43) return new DeviceReport(report); // returns DeviceReport because of not supported yet

            return new DeviceReport(report);
        }
    }
}

