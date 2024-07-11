using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public class XP_PenGen2ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);
            if ((report[1] & 0xF0) == 0xA0)
                return new XP_PenTabletGen2Report(report);
            return new DeviceReport(report);
        }
    }
}
