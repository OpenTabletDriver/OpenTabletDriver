using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public struct InspiroyRelWheelReport(byte[] data) : IRelativeWheelReport
    {
        public byte[] Raw { get; set; } = data;
        public int[] AnalogDeltas { get; set; } = [ParseWheelDelta(data[5])];

        private static int ParseWheelDelta(byte wheelData) =>
            wheelData switch
            {
                0x1 => 1,
                0x2 => -1,
                _ => 0,
            };
    }
}

