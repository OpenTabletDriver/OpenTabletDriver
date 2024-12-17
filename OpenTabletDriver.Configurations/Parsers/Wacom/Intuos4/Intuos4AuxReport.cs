using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4AuxReport : IAuxReport, IAbsoluteWheelReport
    {
        private const uint WHEEL_STEPS = 71;
        private const double HALF_WHEEL_STEPS = WHEEL_STEPS / 2d;
        private const double TREE_HALF_WHEEL_STEPS = HALF_WHEEL_STEPS * 3;

        public Intuos4AuxReport(byte[] report, ref uint? prevWheelPosition)
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

            // Wheel Start at Position zero (0x80) and Provides a value between 0x80 & 0xC7 on PTK 440, 640 & 840
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
        public int? Delta { get; set; }
        public bool[] WheelButtons { get; set; }

        private static int? CalculateDelta(uint? from, uint? to)
        {
            return (int?)(((to - from + TREE_HALF_WHEEL_STEPS) % WHEEL_STEPS) - HALF_WHEEL_STEPS);
        }
    }
}
