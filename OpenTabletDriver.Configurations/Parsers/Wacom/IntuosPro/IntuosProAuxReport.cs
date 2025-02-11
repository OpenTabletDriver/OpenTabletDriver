using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public struct IntuosProAuxReport : IAuxReport, IWheelButtonReport, IAbsoluteWheelReport
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

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTH-x50 & PTH-x51     
            if (wheelByte.IsBitSet(7))
                Position = (uint)wheelByte - 0x80;

            WheelButtons = new bool[]
            {
                touchWheelButtonByte.IsBitSet(0),
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint? Position { set; get; }
        public bool[] WheelButtons { set; get; }
    }
}
