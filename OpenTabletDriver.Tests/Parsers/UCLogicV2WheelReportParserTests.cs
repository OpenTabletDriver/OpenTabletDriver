using OpenTabletDriver.Configurations.Parsers.UCLogic;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;
using Xunit;

namespace OpenTabletDriver.Tests.Parsers
{
    public class UCLogicV2WheelReportParserTests
    {
        [Theory]
        [InlineData(0x01, 11)]
        [InlineData(0x0B, 1)]
        [InlineData(0x0C, 0)]
        public void Parses_Wheel_Report_With_Clockwise_Increasing_Position(byte rawPosition, uint expectedPosition)
        {
            var parser = new UCLogicV2WheelReportParser();
            var report = parser.Parse(CreateWheelReport(rawPosition));

            var wheelReport = Assert.IsAssignableFrom<IAbsoluteWheelReport>(report);
            Assert.Equal(new uint?[] { expectedPosition }, wheelReport.AnalogPositions);
        }

        [Fact]
        public void Parses_Idle_Wheel_Report_As_Null_Position()
        {
            var parser = new UCLogicV2WheelReportParser();
            var report = parser.Parse(CreateWheelReport(0x00));

            var wheelReport = Assert.IsAssignableFrom<IAbsoluteWheelReport>(report);
            Assert.Equal(new uint?[] { null }, wheelReport.AnalogPositions);
        }

        [Fact]
        public void Preserves_UCLogic_V2_Aux_Report_Parsing()
        {
            var parser = new UCLogicV2WheelReportParser();
            var report = parser.Parse([0x08, 0xE0, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x13, 0xF2]);

            Assert.IsAssignableFrom<IAuxReport>(report);
        }

        [Fact]
        public void Preserves_UCLogic_V2_Tablet_Report_Parsing()
        {
            var parser = new UCLogicV2WheelReportParser();
            var report = parser.Parse([0x08, 0x80, 0x40, 0x1F, 0x20, 0x4E, 0x10, 0x00, 0x00, 0x20, 0x13, 0xF2]);

            Assert.IsAssignableFrom<ITabletReport>(report);
        }

        private static byte[] CreateWheelReport(byte rawPosition) =>
        [
            0x08, 0xF0, 0x01, 0x01, 0x00, rawPosition,
            0x00, 0x00, 0x00, 0x00, 0x13, 0xF2
        ];
    }
}
