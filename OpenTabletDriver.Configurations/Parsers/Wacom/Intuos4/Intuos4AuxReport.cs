using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport, IAbsoluteWheelsReport, IWheelsButtonsReport
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
                buttonsByte.IsBitSet(4),
                buttonsByte.IsBitSet(5),
                buttonsByte.IsBitSet(6),
                buttonsByte.IsBitSet(7),
            };

            var wheelByte = report[1];
            WheelsPosition = new uint?[1];

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTK 440, 640 & 840
            if (wheelByte.IsBitSet(7))
                WheelsPosition[0] = (uint)wheelByte - 0x80;

            WheelsButtons =
            [
                new WheelButtonsStates() {
                    States =
                    [
                        touchWheelButtonByte.IsBitSet(0),
                    ]
                }
            ];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint?[] WheelsPosition { set; get; }
        public WheelButtonsStates[] WheelsButtons { set; get; }
    }
}
