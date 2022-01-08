using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public class XP_PenOffsetPressureReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);

            if (report[1].IsBitSet(4))
                return new XP_PenAuxReport(report);

            if (report.Length >= 12)
                return new XP_PenTabletPressureOffsetOverflowReport(report);
            if (report.Length >= 10)
                return new XP_PenPressureOffsetTiltTabletReport(report);
            else
                return new XP_PenPressureOffsetTabletReport(report);
        }
    }
}
