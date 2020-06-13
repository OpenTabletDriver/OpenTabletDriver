using TabletDriverPlugin.Tablet;
using System.Linq;

namespace TabletDriverLib.Vendors.Vikoo
{
    public struct HK708AuxReport : IAuxReport
    {
        public HK708AuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = Enumerable
                .Range(0, 8)
                .Select(i => (report[4] & (1 << i)) != 0)
                .ToArray();
        }

        public byte[] Raw { private set; get; }
        public bool[] AuxButtons { private set; get; }
    }
}