using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Gaomon
{
    public struct GaomonM6WheelReport : IAbsoluteWheelReport
    {
        public GaomonM6WheelReport(byte[] data)
        {
            Raw = data;
            byte wheelData = data[5];
            // Map hardware range 1~12 to framework range 0~11 for AbsoluteWheelMax: 11.
            // null means no touch on ring.
            AnalogPositions = [wheelData != 0 ? wheelData-1u : null];
        }

        public byte[] Raw { get; set; }
        public uint?[] AnalogPositions { get; set; }
    }
}