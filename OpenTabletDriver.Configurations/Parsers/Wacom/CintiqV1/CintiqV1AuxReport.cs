using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.CintiqV1
{
    public struct CintiqV1AuxReport : IAuxReport
    {
        public CintiqV1AuxReport(byte[] report)
        {
            Raw = report;

            // TODO: Handle touch strips.
            var leftRadialButton = report[5];
            var leftButtons = report[6];
            var rightRadialButton = report[7];
            var rightButtons = report[8];
            var topButtons = report[9];
            AuxButtons = new bool[]
            {
                leftRadialButton.IsBitSet(0),
                leftButtons.IsBitSet(0),
                leftButtons.IsBitSet(1),
                leftButtons.IsBitSet(2),
                leftButtons.IsBitSet(3),
                leftButtons.IsBitSet(4),
                leftButtons.IsBitSet(5),
                leftButtons.IsBitSet(6),
                leftButtons.IsBitSet(7),
                leftButtons.IsBitSet(8),
                rightRadialButton.IsBitSet(0),
                rightButtons.IsBitSet(0),
                rightButtons.IsBitSet(1),
                rightButtons.IsBitSet(2),
                rightButtons.IsBitSet(3),
                rightButtons.IsBitSet(4),
                rightButtons.IsBitSet(5),
                rightButtons.IsBitSet(6),
                rightButtons.IsBitSet(7),
                rightButtons.IsBitSet(8),
                topButtons.IsBitSet(0), // i-button
                topButtons.IsBitSet(1), // wrench-button
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
