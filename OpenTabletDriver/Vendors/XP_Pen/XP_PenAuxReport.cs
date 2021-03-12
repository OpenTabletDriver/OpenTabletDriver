using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport
    {
        public XP_PenAuxReport(byte[] report)
        {
            Raw = report;
            var ReportID = (uint)report[1] >> 1;
            var ButtonInt = (uint)report[2];
            AuxButtons = new bool[6];
            if (ReportID == 120) 
            {
                AuxButtons = new bool[] 
                {
                    ButtonInt == 1,
                    ButtonInt == 2,
                    ButtonInt == 4,
                    ButtonInt == 8,
                    ButtonInt == 16,
                    ButtonInt == 32
                };
            }
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}