using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public struct UCLogicWheelReport : IAbsoluteWheelReport
    {
        public UCLogicWheelReport(byte[] data)
        {
            Raw = data;
            var pos = data[5];
            // pos == 0 means the ring is not being touched; subtract 1 to make it 0-indexed
            AnalogPositions = [pos != 0 ? pos - 1u : null];
        }

        public byte[] Raw { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
