using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public struct IntuosProAuxReport : IAuxReport, IAbsoluteWheelsReport, IWheelsButtonsReport
    {
        public IntuosProAuxReport(byte[] report)
        {
            Raw = report;

            var touchWheelButtonByte = report[3];
            var auxByte = report[4];

            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(0),
                auxByte.IsBitSet(1),
                auxByte.IsBitSet(2),
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            };

            var wheelByte = report[2];
            WheelsPosition = new uint?[1];

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTH-x50 & PTH-x51     
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
