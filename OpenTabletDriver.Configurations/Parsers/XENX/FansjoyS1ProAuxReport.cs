using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XENX
{
    public struct FansjoyS1ProAuxReport : IAuxReport
    {
        private const int ButtonCount = 18;

        public FansjoyS1ProAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[ButtonCount];

            for (var index = 0; index < AuxButtons.Length; index++)
                AuxButtons[index] = report[index + 2] != 0;
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
