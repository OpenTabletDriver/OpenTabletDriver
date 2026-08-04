using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Gaomon
{
    public struct GaomonM6WheelReport : IAbsoluteWheelReport
    {
        public GaomonM6WheelReport(byte[] data)
        {
            Raw = data;
            byte wheelData = data[5];
            AnalogPositions = [wheelData != 0 ? wheelData-1u : null];
        }

        public byte[] Raw { get; set; }
        public uint?[] AnalogPositions { get; set; }
    }
}