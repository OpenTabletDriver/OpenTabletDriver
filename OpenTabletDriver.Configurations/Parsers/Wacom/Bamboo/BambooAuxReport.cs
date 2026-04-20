using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Bamboo
{
    public struct BambooAuxReport : IAuxReport, IAbsoluteWheelReport
    {
        public BambooAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[7];
            AuxButtons =
            [
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
            ];

            AnalogPositions = [report[8].IsBitSet(7) ? (uint)(report[8] & 0x7f) : null];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
