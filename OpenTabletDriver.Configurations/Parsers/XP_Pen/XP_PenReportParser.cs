using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public class XP_PenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0 || report[1] == 0xF2)
                // Report[1] == 0xF2 seems to be some sort of an error state for
                // UGEE S1060W Specifically the string 02 F2 01 64 01
                return new OutOfRangeReport(report);

            if (report[1].IsBitSet(4))
                return new XP_PenAuxReport(report);

            if (report.Length >= 12)
                return new XP_PenTabletOverflowReport(report);
            else if (report.Length >= 10)
                return new XP_PenTabletReport(report);
            else
                return new TabletReport(report);
        }
    }
}
