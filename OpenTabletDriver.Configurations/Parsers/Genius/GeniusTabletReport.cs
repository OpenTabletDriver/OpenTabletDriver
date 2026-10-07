using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Genius
{
    public struct GeniusTabletReport : ITabletReport
    {
        public GeniusTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Pressure = report[1].IsBitSet(0) ? Unsafe.ReadUnaligned<ushort>(ref report[6]) : 0u;

            PenButtons =
            [
                report[1].IsBitSet(3),
                report[1].IsBitSet(4),
            ];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
