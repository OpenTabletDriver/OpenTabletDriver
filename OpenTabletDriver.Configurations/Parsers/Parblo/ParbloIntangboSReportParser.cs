using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.XP_Pen;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Parblo
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class ParbloIntangboSParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport? Parse(byte[]? report)
        {
            if (report == null || report.Length == 0)
                return null;
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);
            if (report[1].IsBitSet(4))
                return new ParbloIntangboSAuxReport(report);
            if (report.Length >= 12)
                return new XP_PenTabletOverflowReport(report);
            else if (report.Length >= 10)
                return new XP_PenTabletReport(report);
            else
                return new TabletReport(report);
        }
    }
}
