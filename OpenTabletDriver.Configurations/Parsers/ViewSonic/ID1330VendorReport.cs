using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    public struct ID1330VendorReport : ITabletReport, ITiltReport, IEraserReport
    {
        public ID1330VendorReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };

            var penByte = report[9];
            Pressure = penByte.IsBitSet(2) ? Unsafe.ReadUnaligned<ushort>(ref report[10]) : 0u;
            Tilt = new Vector2(report[12] - 90, report[13] - 90);
            PenButtons =
            [
                penByte.IsBitSet(3),
                penByte.IsBitSet(4),
            ];
            Eraser = penByte.IsBitSet(5);
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { set; get; }
        public bool[] PenButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
