using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class XP_PenM908ReportParser : IReportParser<IDeviceReport>
    {
        private bool _lastReportWasWheelRotation = false;

        public IDeviceReport Parse(byte[] report)
        {
            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);

            // Check if its the wheel scroll report of M908 (starts with 02 F0)
            if (report.Length >= 12 && report[0] == 0x02 && report[1] == 0xF0)
            {
                // Report[7] = 0x01 (clockwise) or 0x02(counterclockwise)
                if (report[7] == 0x01 || report[7] == 0x02)
                {
                    _lastReportWasWheelRotation = true;
                    return new XP_PenM908WheelReport(report);
                }

                // Reset report: report[7] = 0x00
                if (report[7] == 0x00)
                {
                    if (_lastReportWasWheelRotation)
                    {
                        // This is the reset report immediately following a wheel rotation;
                        // do not update auxiliary button states
                        _lastReportWasWheelRotation = false;
                        return new XP_PenM908WheelReport(report); // AnalogDeltas = [0]
                    }
                    // Otherwise, this is a normal auxiliary button report;
                    // let XP_PenAuxReport handle it
                }
            }

            // Not a wheel report; reset state
            _lastReportWasWheelRotation = false;

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
