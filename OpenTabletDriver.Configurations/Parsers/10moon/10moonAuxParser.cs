using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public struct TenMoonAuxReport : IAuxReport
    {
        public TenMoonAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons =
            [
                !report[12].IsBitSet(0), !report[12].IsBitSet(5),
                !report[12].IsBitSet(1), !report[12].IsBitSet(4),
                !report[11].IsBitSet(6), !report[11].IsBitSet(7),
                !report[11].IsBitSet(5), !report[11].IsBitSet(4),
            ];
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
