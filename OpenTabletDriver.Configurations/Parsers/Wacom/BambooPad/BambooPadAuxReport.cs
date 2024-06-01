using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.BambooPad
{
    public struct BambooPadAuxReport : IAuxReport
    {
        public BambooPadAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[23] == 1,
                report[23] == 2
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
