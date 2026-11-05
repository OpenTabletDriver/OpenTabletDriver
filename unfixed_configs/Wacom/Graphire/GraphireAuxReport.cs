using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Graphire
{
    public struct GraphireAuxReport : IAuxReport
    {
        public GraphireAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[7];
            AuxButtons =
            [
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            ];

            // wheel = report[7][5:3]
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
