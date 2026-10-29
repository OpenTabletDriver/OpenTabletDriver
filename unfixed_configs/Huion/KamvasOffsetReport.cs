using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public struct KamvasOffsetReport : ITabletReport, ITiltReport
    {
        internal KamvasOffsetReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]) | ((report[4] & 1) << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10] * -1,
                Y = (sbyte)report[11] * -1
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[7]);

            PenButtons =
            [
                report[1].IsBitSet(1),
                report[1].IsBitSet(2),
                report[1].IsBitSet(3),
            ];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
