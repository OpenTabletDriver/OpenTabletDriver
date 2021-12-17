using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Genius
{
    public struct GeniusButtonStripAuxReport : IAuxReport
    {
        public GeniusButtonStripAuxReport(byte[] report)
        {
            Raw = report;

            var auxByte = report[3];
            AuxButtons = CreateButton(auxByte);
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }

        private static bool[] CreateButton(byte auxByte)
        {
            var aux = new bool[12];

            var activeIndex = (auxByte - 1) / 2;
            aux[activeIndex] = true;

            return aux;
        }
    }
}
