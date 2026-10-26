using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport, IWheelButtonReport, IAbsoluteWheelReport
    {
        public Intuos4AuxReport(byte[] report)
        {
            Raw = report;

            var touchWheelButtonByte = report[2];
            var buttonsByte = report[3];

            AuxButtons =
            [
                buttonsByte.IsBitSet(0),
                buttonsByte.IsBitSet(1),
                buttonsByte.IsBitSet(2),
                buttonsByte.IsBitSet(3),
                buttonsByte.IsBitSet(4),
                buttonsByte.IsBitSet(5),
                buttonsByte.IsBitSet(6),
                buttonsByte.IsBitSet(7),
            ];

            var wheelByte = report[1];

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTK 440, 640 & 840
            AnalogPositions = [wheelByte.IsBitSet(7) ? (uint)(wheelByte & 0x7f) : null];

            WheelButtons = [[touchWheelButtonByte.IsBitSet(0)]];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool[][] WheelButtons { get; set; }
        public uint?[] AnalogPositions { get; set; }
    }
}
