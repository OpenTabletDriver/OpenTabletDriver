using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Euronovate
{
    public struct ENSignTabletReport : ITabletReport, ITiltReport, IEraserReport
    {
        public ENSignTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[7]);

            Tilt = new Vector2
            {
                X = Unsafe.ReadUnaligned<short>(ref report[9]),
                Y = Unsafe.ReadUnaligned<short>(ref report[11])
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
