using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Genius
{
    public struct GeniusMouseReport : IMouseReport
    {
        public GeniusMouseReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            MouseButtons = new bool[]
            {
                report[1].IsBitSet(0), // primary
                report[1].IsBitSet(1), // secondary
                report[1].IsBitSet(2), // middle
            };
            Scroll = new Vector2
            {
                Y = (sbyte)report[6]
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public bool[] MouseButtons { set; get; }
        public Vector2 Scroll { set; get; }
    }
}
