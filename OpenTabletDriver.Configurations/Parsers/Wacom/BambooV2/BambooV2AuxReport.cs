using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.BambooV2
{
    public struct BambooV2AuxReport : IAuxReport
    {
        public BambooV2AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[7];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(6), // CTE-440 left
                auxByte.IsBitSet(7), // CTE-440 right
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
