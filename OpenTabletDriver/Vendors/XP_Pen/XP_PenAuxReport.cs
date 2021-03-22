using System.Collections.Specialized;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport
    {
        public XP_PenAuxReport(byte[] report)
        {
            Raw = report;

            var bitVector = new BitVector32(report[2]);
            AuxButtons = new bool[]
            {
                bitVector[1 << 0],
                bitVector[1 << 1],
                bitVector[1 << 2],
                bitVector[1 << 3],
                bitVector[1 << 4],
                bitVector[1 << 5],
                bitVector[1 << 6],
                bitVector[1 << 7]
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
