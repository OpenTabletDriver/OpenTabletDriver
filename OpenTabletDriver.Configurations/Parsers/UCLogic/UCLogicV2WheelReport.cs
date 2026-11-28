using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    /// <summary>
    /// An absolute wheel report for UCLogic v2 protocol tablets.
    /// </summary>
    /// <remarks>
    /// The wheel value is reported in <c>report[5]</c> of a <c>0xf0</c> report:
    /// <c>0x00</c> indicates the wheel is not being touched, and <c>0x01</c>-<c>0x0c</c>
    /// map to the 12 wheel positions.
    /// <para/>
    /// The raw value counts <i>down</i> on clockwise rotation (e.g. Gaomon M6),
    /// so it is inverted here to keep positive position deltas == clockwise rotation.
    /// </remarks>
    public struct UCLogicV2WheelReport : IAbsoluteWheelReport
    {
        public UCLogicV2WheelReport(byte[] report)
        {
            Raw = report;
            var wheelData = report[5];

            // 0x00 = released; positions are 0x01..0x0c (12 steps), inverted
            // so that a physically clockwise turn yields increasing positions.
            AnalogPositions = [wheelData != 0 ? (uint)(12 - wheelData) : null];
        }

        public byte[] Raw { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
