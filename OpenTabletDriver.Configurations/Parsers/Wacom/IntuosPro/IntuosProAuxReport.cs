using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public struct IntuosProAuxReport : IAuxReport, IWheelButtonReport, IAbsoluteWheelReport
    {
        private const int WHEEL_STEPS = 71;
        private const double HALF_WHEEL_STEPS = 71 / 2d;
        private const double TREE_HALF_WHEEL_STEPS = HALF_WHEEL_STEPS * 3;

        public IntuosProAuxReport(byte[] report, ref uint? prevWheelPosition)
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
            {
                Position = (uint)wheelByte - 0x80;
                Delta = CalculateDelta(prevWheelPosition, Position);
            }

            WheelButtons = new bool[]
            {
                touchWheelButtonByte.IsBitSet(0),
            };

            prevWheelPosition = Position;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public uint? Position { set; get; }
        public int? Delta { set; get; }
        public bool[] WheelButtons { set; get; }

        private int? CalculateDelta(uint? from, uint? to)
        {
            return (int?)(((to - from + TREE_HALF_WHEEL_STEPS) % WHEEL_STEPS) - HALF_WHEEL_STEPS);
        }
    }
}
