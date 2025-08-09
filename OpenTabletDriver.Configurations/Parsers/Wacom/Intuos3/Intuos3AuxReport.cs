using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos3
{
    public struct Intuos3AuxReport : IAuxReport
    {
        public Intuos3AuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                report[5].IsBitSet(0),
                report[5].IsBitSet(1),
                report[5].IsBitSet(2),
                report[5].IsBitSet(3),
                report[6].IsBitSet(0),
                report[6].IsBitSet(1),
                report[6].IsBitSet(2),
                report[6].IsBitSet(3)
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
