using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class IntuosV3AuxReport : IAuxReport
    {
        public IntuosV3AuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                (report[0] & (1 << 0)) != 0,
                (report[0] & (1 << 1)) != 0,
                (report[0] & (1 << 2)) != 0,
                (report[0] & (1 << 3)) != 0
            };
        }
        
        public byte[] Raw { private set; get; }
        public bool[] AuxButtons { private set; get; }
    }
}