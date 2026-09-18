using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

// The Deco 03 requires additional logic for parsing
// the wheel reports. To prevent issues with other tablets,
// a seperate parser has been made for the Deco 03

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class XP_PenDeco03ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);

            if (report[1] == 0xF0)
            {
                return new XP_PenDeco03AuxReport(report, ref previousWheelByte);
            }

            if (report.Length >= 12)
                return new XP_PenTabletOverflowReport(report);
            else if (report.Length >= 10)
                return new XP_PenTabletReport(report);
            else
                return new TabletReport(report);
        }
        private byte previousWheelByte;
    }
}
