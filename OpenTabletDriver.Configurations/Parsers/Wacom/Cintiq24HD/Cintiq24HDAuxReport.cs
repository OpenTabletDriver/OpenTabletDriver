using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Cintiq24HD
{
    public struct Cintiq24HDAuxReport : IAuxReport, IAbsoluteWheelReport
    {
        public Cintiq24HDAuxReport(byte[] report)
        {
            Raw = report;

            var leftButtons = report[6];
            var rightButtons = report[8];
            var infoButton = report[3];
            var topButtons = report[4];
            AuxButtons =
            [
                leftButtons.IsBitSet(0),
                leftButtons.IsBitSet(1),
                leftButtons.IsBitSet(2),
                leftButtons.IsBitSet(3),
                leftButtons.IsBitSet(4),
                leftButtons.IsBitSet(5),
                leftButtons.IsBitSet(6),
                leftButtons.IsBitSet(7),
                rightButtons.IsBitSet(0),
                rightButtons.IsBitSet(1),
                rightButtons.IsBitSet(2),
                rightButtons.IsBitSet(3),
                rightButtons.IsBitSet(4),
                rightButtons.IsBitSet(5),
                rightButtons.IsBitSet(6),
                rightButtons.IsBitSet(7),
                infoButton.IsBitSet(4), // i-button
                topButtons.IsBitSet(6), // keyboard button
                topButtons.IsBitSet(0), // settings button
            ];

            // Touch rings: bit 7 is set while a finger is on the ring, the low 7 bits are the position (0-71).
            AnalogPositions = [GetRingPosition(report[1]), GetRingPosition(report[2])];
        }

        private static uint? GetRingPosition(byte ring)
        {
            return ring.IsBitSet(7) ? (uint)(ring & 0x7F) : null;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint?[] AnalogPositions { get; set; }
    }
}
