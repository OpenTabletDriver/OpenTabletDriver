using System;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public class VeikkTiltedReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            // Tablet Report
            if (report[1] == 0x41) {
                // throw exception if report length less than 13 byte
                if (report.Length < 13) throw new ArgumentOutOfRangeException("byte[] report", report, "The report length less than expected length which is 13");
                    
                // out of range report
                if (report[2] == 0xC0) return new OutOfRangeReport(report);

                // Veikk tilted tablet report
                return new VeikkTiltedTabletReport(report);
            }

            
            // Aux Report
            if (report[1] == 0x42) return new VeikkAuxReport(report);

            // Touchpad Report
            if (report[1] == 0x43) return new DeviceReport(report); // returns DeviceReport because of not supported yet

            // Unknown Report
            return new DeviceReport(report);
        }
    }
}
