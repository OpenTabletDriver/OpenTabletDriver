using OpenTabletDriver.Configurations.Parsers.Huion;
using OpenTabletDriver.Configurations.Parsers.UCLogic;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class KamvasPro16ReportParserTest
    {
        private static byte[] StripReport(byte stripPosition, byte auxButtonByte = 0x00)
        {
            // 12-byte auxiliary report: [1]=0xF0 (strip), [4]=aux buttons, [5]=strip position
            var report = new byte[12];
            report[0] = 0x08;
            report[1] = 0xF0;
            report[2] = 0x01;
            report[3] = 0x01;
            report[4] = auxButtonByte;
            report[5] = stripPosition;
            report[10] = 0x15;
            report[11] = 0xEF;
            return report;
        }

        [Theory]
        // Touch bar position -> absolute wheel position (1-7 -> 0-6), 0 = released -> null
        [InlineData(0x00, null)]
        [InlineData(0x01, 0u)]
        [InlineData(0x04, 3u)]
        [InlineData(0x07, 6u)]
        public void StripPosition_Is_Mapped_To_Absolute_Wheel_Position(byte stripPosition, uint? expected)
        {
            var report = (KamvasPro16AuxStripReport)new KamvasPro16ReportParser().Parse(StripReport(stripPosition));

            Assert.NotNull(report.AnalogPositions);
            Assert.Single(report.AnalogPositions);
            Assert.Equal(expected, report.AnalogPositions[0]);
        }

        [Fact]
        public void Released_Reports_Null_Position()
        {
            var report = (KamvasPro16AuxStripReport)new KamvasPro16ReportParser().Parse(StripReport(0x00));

            Assert.Null(report.AnalogPositions[0]);
        }

        [Fact]
        public void UserCapture_Parses_To_Position_6_With_No_Aux_Buttons()
        {
            // Raw capture while touching the bottom of the strip (position 7 -> segment index 6)
            var report = (KamvasPro16AuxStripReport)new KamvasPro16ReportParser().Parse(
                new byte[] { 0x08, 0xF0, 0x01, 0x01, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x15, 0xEF });

            Assert.NotNull(report.AnalogPositions[0]);
            Assert.Equal(6u, report.AnalogPositions[0]!.Value);
            Assert.DoesNotContain(true, report.AuxButtons);
        }

        [Fact]
        public void AuxButtons_Are_Read_From_Byte4()
        {
            var report = (KamvasPro16AuxStripReport)new KamvasPro16ReportParser().Parse(StripReport(0x07, auxButtonByte: 0x3F));

            // Bits 0-5 (the six physical buttons) set, bits 6-7 clear
            var expected = new[] { true, true, true, true, true, true, false, false };
            Assert.Equal(expected, report.AuxButtons);
        }

        [Fact]
        public void RelativeWheel_Report_Is_Still_Dispatched()
        {
            var report = new KamvasPro16ReportParser().Parse(
                new byte[] { 0x08, 0xF1, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            var wheel = Assert.IsType<KamvasRelWheelReport>(report);
            Assert.Equal(new[] { 1, 0 }, wheel.AnalogDeltas);
        }

        [Fact]
        public void AuxButtonReport_Is_Still_Dispatched()
        {
            // data[1] bits 5+6 set -> auxiliary button report (not the strip path)
            var report = new KamvasPro16ReportParser().Parse(
                new byte[] { 0x08, 0x60, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            Assert.IsType<UCLogicAuxReport>(report);
        }

        [Fact]
        public void Pen_Report_Is_Still_Dispatched()
        {
            var report = new KamvasPro16ReportParser().Parse(
                new byte[] { 0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            Assert.IsType<GianoReport>(report);
        }
    }
}
