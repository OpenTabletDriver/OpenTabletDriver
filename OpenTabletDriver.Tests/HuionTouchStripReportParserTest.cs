using System;
using System.Linq;
using OpenTabletDriver.Configurations.Parsers.Huion;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;
using Xunit;

namespace OpenTabletDriver.Tests
{
    // Reports recorded from a Huion Kamvas Pro 13 (HUION_M20k_220302).
    public class HuionTouchStripReportParserTest
    {
        private readonly HuionTouchStripReportParser _parser = new();

        [Theory]
        // Touch strip positions 1..7 (top to bottom) map to wheel positions 0..6.
        [InlineData("08 F0 01 01 00 01 00 00 00 00 0A FF", 0u)]
        [InlineData("08 F0 01 01 00 04 00 00 00 00 0A FF", 3u)]
        [InlineData("08 F0 01 01 00 07 00 00 00 00 0A FF", 6u)]
        // Finger lifted
        [InlineData("08 F0 01 01 00 00 00 00 00 00 0A FF", null)]
        public void TouchStrip_Is_Parsed_As_Absolute_Wheel(string report, uint? expectedPosition)
        {
            var parsed = _parser.Parse(ToBytes(report));

            var wheelReport = Assert.IsAssignableFrom<IAbsoluteWheelReport>(parsed);
            Assert.Equal(new[] { expectedPosition }, wheelReport.AnalogPositions);
        }

        [Fact]
        public void TouchStrip_Report_Does_Not_Affect_Aux_Buttons()
        {
            // Byte 4 is zero in touch strip reports, so treating them as aux reports
            // would release any express key held while using the strip.
            var parsed = _parser.Parse(ToBytes("08 F0 01 01 00 03 00 00 00 00 07 00"));

            Assert.IsNotAssignableFrom<IAuxReport>(parsed);
        }

        [Theory]
        [InlineData("08 E0 01 01 01 00 00 00 00 00 0F 00", 0)]
        [InlineData("08 E0 01 01 02 00 00 00 00 00 0F 00", 1)]
        [InlineData("08 E0 01 01 04 00 00 00 00 00 11 00", 2)]
        [InlineData("08 E0 01 01 08 00 00 00 00 00 11 00", 3)]
        public void Express_Keys_Are_Parsed_As_Aux_Buttons(string report, int pressedButton)
        {
            var parsed = _parser.Parse(ToBytes(report));

            var auxReport = Assert.IsAssignableFrom<IAuxReport>(parsed);
            var pressed = auxReport.AuxButtons
                .Select((isPressed, index) => (isPressed, index))
                .Where(button => button.isPressed)
                .Select(button => button.index);
            Assert.Equal(new[] { pressedButton }, pressed);
        }

        [Fact]
        public void Pen_Report_Is_Parsed_As_Tilt_Report()
        {
            var parsed = _parser.Parse(ToBytes("08 81 D7 99 0D 34 20 09 00 00 05 00"));

            var tabletReport = Assert.IsType<TiltTabletReport>(parsed);
            Assert.Equal(39383f, tabletReport.Position.X);
            Assert.Equal(13325f, tabletReport.Position.Y);
            Assert.Equal(2336u, tabletReport.Pressure);
        }

        private static byte[] ToBytes(string hex) =>
            hex.Split(' ').Select(b => Convert.ToByte(b, 16)).ToArray();
    }
}
