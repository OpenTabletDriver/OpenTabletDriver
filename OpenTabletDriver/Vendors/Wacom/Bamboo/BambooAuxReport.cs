using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom.Bamboo
{
    public struct BambooAuxReport : IAuxReport
    {
        public BambooAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[7].IsBitSet(3),
                report[7].IsBitSet(4),
                report[7].IsBitSet(5),
                report[7].IsBitSet(6),
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get;  }
    }
}