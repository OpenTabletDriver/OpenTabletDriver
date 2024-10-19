using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport, IAbsoluteWheelReport
    {
        public Intuos4AuxReport(byte[] report)
        {
            Raw = report;

            var touchWheelButtonByte = report[2];
            var buttonsByte = report[3];

            AuxButtons = new bool[]
            {
                buttonsByte.IsBitSet(0),
                buttonsByte.IsBitSet(1),
                buttonsByte.IsBitSet(2),
                buttonsByte.IsBitSet(3),

                touchWheelButtonByte.IsBitSet(0),

                buttonsByte.IsBitSet(4),
                buttonsByte.IsBitSet(5),
                buttonsByte.IsBitSet(6),
                buttonsByte.IsBitSet(7),
            };

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTK 440, 640 & 840
            if (report[2].IsBitSet(7))
                WheelPosition = (uint)report[2] - 0x80;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint? WheelPosition { set; get; }
    }
}
