using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport
    {
        public Intuos4AuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                (report[3] & (1 << 0)) != 0,
                (report[3] & (1 << 1)) != 0,
                (report[3] & (1 << 2)) != 0,
                (report[3] & (1 << 3)) != 0,
                (report[3] & (1 << 4)) != 0,
                (report[3] & (1 << 5)) != 0,
                (report[3] & (1 << 6)) != 0,
                (report[3] & (1 << 7)) != 0
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
