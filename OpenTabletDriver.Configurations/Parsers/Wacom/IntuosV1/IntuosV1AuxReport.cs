using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public struct IntuosV1AuxReport : IAuxReport, IAbsoluteWheelReport
    {
        public IntuosV1AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[4];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            };

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTH-x50 & PTH-x51     
            if (report[2].IsBitSet(7))
                WheelPosition = (uint)report[2] - 0x80;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint? WheelPosition { set; get; }
    }
}
