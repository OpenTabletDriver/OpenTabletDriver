using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class XP_PenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);

            // The tablet throw inits sent its way back to us, we ignore them.
            if (report[1] == 0xB4)
                return new DeviceReport(report);

            // Ignore idle reports, as they were parsed as aux reports previously.
            // see https://github.com/OpenTabletDriver/OpenTabletDriver/issues/4988 for more details
            if (report[1] == 0xF2)
                return new DeviceReport(report);

            // Ignore ON report when toggling ON tablets using the wireless dongle from XP-Pen.
            // see https://github.com/OpenTabletDriver/OpenTabletDriver/issues/4988 for more details
            if (report[1] == 0xF8)
                return new XP_PenAuxReport(report);

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
