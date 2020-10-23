using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Gaomon
{
    public struct GaomonAuxReport : IAuxReport
    {
        public GaomonAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                (report[4] & (1 << 0)) != 0,
                (report[4] & (1 << 1)) != 0,
                (report[4] & (1 << 2)) != 0,
                (report[4] & (1 << 3)) != 0
            };
            AuxWheel = report[5];
        }

        public bool[] AuxButtons { private set; get; }
        public byte[] Raw { private set; get; }
        public int AuxWheel { private set; get; }
    }
}