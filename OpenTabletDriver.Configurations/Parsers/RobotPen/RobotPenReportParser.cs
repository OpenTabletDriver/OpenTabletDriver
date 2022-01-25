using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.RobotPen
{
    public class RobotPenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[5].ToString() == "16" || report[5].ToString() == "17")
                return new RobotPenTabletReport(report);

            return new DeviceReport(report);
        }
    }
}
