using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2AuxReport : IAuxReport
    {
        public IntuosV2AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[1];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
