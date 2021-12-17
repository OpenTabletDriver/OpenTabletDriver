using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    public struct UCLogicAuxReport : IAuxReport
    {
        public UCLogicAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[4].IsBitSet(0),
                report[4].IsBitSet(1),
                report[4].IsBitSet(2),
                report[4].IsBitSet(3),
                report[4].IsBitSet(4),
                report[4].IsBitSet(5),
                report[4].IsBitSet(6),
                report[4].IsBitSet(7),
                report[5].IsBitSet(0),
                report[5].IsBitSet(1),
                report[5].IsBitSet(2),
                report[5].IsBitSet(3),
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
