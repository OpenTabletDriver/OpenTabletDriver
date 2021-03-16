using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport
    {
        public XP_PenAuxReport(byte[] report)
        {
            Raw = report;
            var ReportID = (uint)report[1] >> 1;
            AuxButtons = new bool[6];
            if (ReportID == 120) 
            {
                AuxButtons = new bool[] 
                {
                    (report[2] & (1 << 0)) != 0,
                    (report[2] & (1 << 1)) != 0,
                    (report[2] & (1 << 2)) != 0,
                    (report[2] & (1 << 3)) != 0,
                    (report[2] & (1 << 4)) != 0,
                    (report[2] & (1 << 5)) != 0
                };
            }
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
