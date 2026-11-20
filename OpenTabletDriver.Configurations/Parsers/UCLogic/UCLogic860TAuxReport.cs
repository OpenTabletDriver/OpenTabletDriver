using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    // Gaomon 860T reports its 6 aux buttons on bits 0,1,2,5,6,7 of report[4];
    // bits 3 and 4 are unused. This compacts them into a dense 6-button array.
    public struct UCLogic860TAuxReport : IAuxReport
    {
        public UCLogic860TAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons =
            [
                report[4].IsBitSet(0),
                report[4].IsBitSet(1),
                report[4].IsBitSet(2),
                report[4].IsBitSet(5),
                report[4].IsBitSet(6),
                report[4].IsBitSet(7),
            ];
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
