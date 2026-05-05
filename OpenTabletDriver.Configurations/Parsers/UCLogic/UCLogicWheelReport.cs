using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public struct UCLogicWheelReport : IAbsoluteWheelReport
    {
        private const byte RawMinPosition = 1;
        private const byte RawMaxPosition = 12;

        public UCLogicWheelReport(byte[] report)
        {
            Raw = report;

            var wheelData = report[5];

            uint? position = wheelData is >= RawMinPosition and <= RawMaxPosition
                ? (uint)(RawMaxPosition - wheelData)
                : null;
            AnalogPositions = [position];
        }

        public uint?[] AnalogPositions { set; get; }
        public byte[] Raw { set; get; }
    }
}
