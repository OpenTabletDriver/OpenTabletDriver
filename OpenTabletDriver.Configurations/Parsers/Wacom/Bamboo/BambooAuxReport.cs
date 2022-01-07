using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Bamboo
{
    public struct BambooAuxReport : IAuxReport
    {
        public BambooAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[7];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
            };

            // wheel = report[8] & 0x7f
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
