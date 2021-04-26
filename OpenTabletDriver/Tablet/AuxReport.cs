using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tablet
{
    public struct AuxReport : IAuxReport
    {
        public AuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[3];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3)
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}