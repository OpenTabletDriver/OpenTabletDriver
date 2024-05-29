using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public struct XP_PenAuxReport : IAuxReport
    {
        public XP_PenAuxReport(byte[] report, int index = 2)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[index].IsBitSet(0),
                report[index].IsBitSet(1),
                report[index].IsBitSet(2),
                report[index].IsBitSet(3),
                report[index].IsBitSet(4),
                report[index].IsBitSet(5),
                report[index].IsBitSet(6),
                report[index].IsBitSet(7),
                report[index + 1].IsBitSet(0),
                report[index + 1].IsBitSet(1),
                report[index + 1].IsBitSet(2),
                report[index + 1].IsBitSet(3),
                report[index + 1].IsBitSet(4),
                report[index + 1].IsBitSet(5),
                report[index + 1].IsBitSet(6),
                report[index + 1].IsBitSet(7),
                report[index + 2].IsBitSet(0),
                report[index + 2].IsBitSet(1),
                report[index + 2].IsBitSet(2),
                report[index + 2].IsBitSet(3),
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
