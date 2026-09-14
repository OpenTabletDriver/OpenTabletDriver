using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class XP_PenM908ReportParser : IReportParser<IDeviceReport>
    {
        private int _wheelResetGraceCounter = 0;

        public IDeviceReport Parse(byte[] report)
        {
            if (_wheelResetGraceCounter > 0)
                _wheelResetGraceCounter = _wheelResetGraceCounter - 1;
            // Ugee M908 tablet uses a really wierd wheel report logic.
            // When rotated one step, the firmware will send a report like: 02 F0 00 00 00 00 00 01(or 02) 00 00 00 00.
            // Which means the wheel is rotated clockwisely(or counterclockwisely). I call it wheel rotate report.
            // Then, the firmware will send a 02 F0 00 00 00 00 00 00 00 00 00 00 report (I call it wheel reset report)
            // in about 5ms. Which will reset all the auxiliary buttons to release. This is not good :(
            //
            // Usually these two reports are next to each other.
            // But sometimes, especially when you put your pen on the tablet, which generates a lot of pen report,
            // other reports will be inserted between those two wheel reports and cause some unexpected issues.
            //
            // So I introduce the _wheelResetGraceCounter to cout how many reports to wait (until the firmware send
            // a wheel reset report) after detecting a wheel rotate report. Once the correspond wheel reset report was
            // detected, the counter will be set to 0, whick means this wheel rotate action is perfectly handeled.
            // So that auxiliary buttons will not affected by the wheel reset report.
            //
            // Overall parsing logic:
            // 1. Decrement the wheel reset grace counter unconditionally to prevent state leaks.
            // 2. If report[1] does not have bit 4 set, it's not an aux/wheel report; fall back to XP_PenReportParser logic.
            // 3. If any auxiliary button is pressed (report[2] != 0 or report[3] != 0), return XP_PenAuxReport.
            // 4. If report[7] != 0, it's a wheel rotation; reset the counter to 3 and return XP_PenM908WheelReport.
            // 5. If report[7] == 0, check the wheel life counter:
            //    - If > 0, it's a wheel reset; set counter to 0 and return XP_PenM908WheelReport.
            //    - If == 0, it's an all-aux-buttons-up report; return XP_PenAuxReport.


            if (report[1] == 0xC0)
                return new OutOfRangeReport(report);

            // Aux/wheel report
            if (report[1].IsBitSet(4))
            {
                // Auxiliary button pressed
                if (report[2] != 0x00 || report[3] != 0x00)
                    return new XP_PenAuxReport(report);

                // Wheel rotated
                if (report[7] != 0x00)
                {
                    // Set the counter
                    _wheelResetGraceCounter = 3;
                    return new XP_PenM908WheelReport(report);
                }

                // report[7] == 0x00 means this is a reset report. Check if wheel was rotated in last 3 steps
                if (_wheelResetGraceCounter > 0)
                {
                    // Wheel reset report
                    _wheelResetGraceCounter = 0;
                    return new XP_PenM908WheelReport(report);
                }

                // All aux buttons up report
                return new XP_PenAuxReport(report);
            }
            // Fallback to XP_PenReportParser logic for pen reports and other misc reports
            if (report.Length >= 12)
                return new XP_PenTabletOverflowReport(report);
            else if (report.Length >= 10)
                return new XP_PenTabletReport(report);
            else
                return new TabletReport(report);
        }
    }
}
