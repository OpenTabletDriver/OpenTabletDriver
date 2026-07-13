using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Bosto
{
    public struct BostoAuxReport : IAuxReport
    {
        public BostoAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[4];
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
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
