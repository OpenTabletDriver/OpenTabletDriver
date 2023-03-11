using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.FlooGoo
{
    public struct FmaTabletReport : ITabletReport, ITiltReport, IEraserReport
    {
        public FmaTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            // Unit: [-9000..9000]x10^-3 degrees
            Tilt = new Vector2
            {
                X = Unsafe.ReadUnaligned<short>(ref report[8]) * 0.01f,
                Y = Unsafe.ReadUnaligned<short>(ref report[10]) * 0.01f
            };

            var penByte = report[1];

            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };

            Eraser = penByte.IsBitSet(3);
        }

        public byte[] Raw { get; set; }
        public Vector2 Position { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }
        public Vector2 Tilt { get; set; }
        public bool Eraser { get; set; }
    }
}
