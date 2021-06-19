using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport
    {
        public XP_PenAuxReport(byte[] report)
        {
            Raw = report;
            var auxByte = report[2];

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
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
