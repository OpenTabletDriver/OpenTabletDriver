using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkAuxReport : IAuxReport
    {
        public VeikkAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                report[4].IsBitSet(0) && report[3].IsBitSet(0), // First
                report[4].IsBitSet(1) && report[3].IsBitSet(0), // Second
                report[4].IsBitSet(2) && report[3].IsBitSet(0), // Third
                report[4].IsBitSet(3) && report[3].IsBitSet(0), // Fourth
            };
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
