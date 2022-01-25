using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.RobotPen
{
    public class RobotPenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1].ToString() == "66")
                return new RobotPenTabletReport(report);

            return new DeviceReport(report);
        }
    }
}
