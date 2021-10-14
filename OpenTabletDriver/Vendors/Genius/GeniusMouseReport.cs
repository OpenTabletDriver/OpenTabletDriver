using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver.Vendors.Genius
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
                report[1].IsBitSet(1), // primary
                report[1].IsBitSet(2), // secondary
                report[1].IsBitSet(3), // middle
                report[6] == 0x01, // forward
                report[6] == 0xFF, // backward
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