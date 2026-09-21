using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    public struct ID1330AuxReport : IAuxReport
    {
        public ID1330AuxReport(byte[] report)
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
            ];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
