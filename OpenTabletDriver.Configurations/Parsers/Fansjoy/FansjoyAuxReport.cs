using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Fansjoy
{
    public struct FansjoyAuxReport : IAuxReport
    {
        public FansjoyAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[2] == 0x05,
                report[3] == 0x19,
                report[4] == 0x08,
                report[5] == 0x0b
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
