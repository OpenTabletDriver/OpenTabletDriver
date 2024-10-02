
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Fansjoy
{
    public class FansjoyReportParser : IReportParser<IDeviceReport>
    {
        private const byte PEN_REPORT_LENGTH = 12;

        public IDeviceReport Parse(byte[] report)
        {
            if (report.Length == PEN_REPORT_LENGTH)
            {
                if (report[1].IsBitSet(5))
                {
                    return new FansjoyTabletReport(report);
                }
                else
                {
                    return null!; // returning null makes OTD ignore this report
                }
            }
            else // report.Length is 14 for aux buttons
            {
                return new FansjoyAuxReport(report);
            }
        }
    }
}
