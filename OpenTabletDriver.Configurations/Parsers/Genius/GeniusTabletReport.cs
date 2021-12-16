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
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[3])
            };
            Pressure = report[5].IsBitSet(2) ? Unsafe.ReadUnaligned<ushort>(ref report[6]) : 0u;

            PenButtons = new bool[]
            {
               report[5].IsBitSet(3),
               report[5].IsBitSet(4)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
