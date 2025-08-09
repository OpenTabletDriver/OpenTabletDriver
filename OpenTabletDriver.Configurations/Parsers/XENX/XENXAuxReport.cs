using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XENX
{
    public struct XENXAuxReport : IAuxReport
    {
        public XENXAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                report[2] != 0,
                report[3] != 0,
                report[4] != 0,
                report[5] != 0,
                report[6] != 0,
                report[7] != 0,
                report[8] != 0,
                report[9] != 0,
                report[10] != 0,
                report[11] != 0,
            };
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
