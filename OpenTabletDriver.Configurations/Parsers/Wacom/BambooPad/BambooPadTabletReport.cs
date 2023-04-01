using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.BambooPad
{
    public struct BambooPadTabletReport : ITabletReport, IEraserReport
    {
        public BambooPadTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };

            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[7]);

            PenButtons = new bool[]
            {
                report[2].IsBitSet(1)
            };

            Eraser = report[2].IsBitSet(3);
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
