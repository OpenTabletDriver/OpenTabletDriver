using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public struct XP_PenM908WheelReport : IRelativeWheelReport
    {
        public XP_PenM908WheelReport(byte[] report)
        {
            Raw = report;

            // Wheel state is located at index 7
            // Clockwise: 0x01 -> +1
            // Counterclockwise: 0x02 -> -1
            // Other values: 0 (no scrolling)
            AnalogDeltas = report[7] switch
            {
                0x01 => [1],
                0x02 => [-1],
                _ => [0]
            };
        }

        public byte[] Raw { get; set; }
        public int[] AnalogDeltas { get; set; }
    }
}
