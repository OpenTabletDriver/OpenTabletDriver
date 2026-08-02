using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    public struct ID1330VendorReport : ITabletReport, ITiltReport, IEraserReport
    {
        private const float VENDOR_MAX_X = 29376f;
        private const float VENDOR_MAX_Y = 16523f;
        private const float TABLET_MAX_X = 30000f;
        private const float TABLET_MAX_Y = 16875f;

        public ID1330VendorReport(byte[] report)
        {
            Raw = report;

            var vendorX = Unsafe.ReadUnaligned<ushort>(ref report[1]);
            var vendorY = Unsafe.ReadUnaligned<ushort>(ref report[5]);
            Position = new Vector2
            {
                X = vendorX * TABLET_MAX_X / VENDOR_MAX_X,
                Y = vendorY * TABLET_MAX_Y / VENDOR_MAX_Y
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
