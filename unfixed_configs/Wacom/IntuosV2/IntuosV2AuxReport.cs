using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2AuxReport : IAuxReport, IAbsoluteWheelReport, IWheelButtonReport
    {
        public IntuosV2AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[1];
            AuxButtons =
            [
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            ];
            WheelButtons = [[report[3].IsBitSet(0)]];
            AnalogPositions = [report[4].IsBitSet(7) ? (uint?)(report[4] & 0x7f) : null];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool[][] WheelButtons { set; get; }
        public uint?[] AnalogPositions { get; set; }
    }
}
