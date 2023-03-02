using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2OffsetReport : ITabletReport, IHoverReport, IConfidenceReport, ITiltReport, IEraserReport
    {
        public IntuosV2OffsetReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]) | (report[5] << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[6]) | (report[8] << 16)
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10],
                Y = (sbyte)report[11]
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[9]);

            var penByte = report[2];
            Eraser = penByte.IsBitSet(4);
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2),
                penByte.IsBitSet(3)
            };
            HighConfidence = report[1].IsBitSet(5);
            HoverDistance = report[11];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
        public bool HighConfidence { set; get; }
        public uint HoverDistance { set; get; }
    }
}