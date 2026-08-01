using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    public struct ID1330TabletReport : ITabletReport, ITiltReport, IEraserReport
    {
        public ID1330TabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            Tilt = new Vector2
            {
                X = Unsafe.ReadUnaligned<short>(ref report[9]) / 100f,
                Y = Unsafe.ReadUnaligned<short>(ref report[11]) / 100f
            };

            var penByte = report[1];
            PenButtons =
            [
                penByte.IsBitSet(1),
            ];
            Eraser = penByte.IsBitSet(2) || penByte.IsBitSet(3);
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { set; get; }
        public bool[] PenButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
