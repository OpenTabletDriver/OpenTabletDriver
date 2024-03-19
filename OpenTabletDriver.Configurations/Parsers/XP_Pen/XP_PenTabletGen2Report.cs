using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XP_Pen
{
    public struct XP_PenTabletGen2Report : ITabletReport, ITiltReport, IEraserReport
    {
        public XP_PenTabletGen2Report(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]) | report[10] << 16,
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4]) | report[11] << 16
            };
            Pressure = (uint)((Unsafe.ReadUnaligned<ushort>(ref report[6]) - 16384) | (report[13] & 0x01) << 13);
            Eraser = report[1].IsBitSet(3);

            PenButtons = new bool[]
            {
                report[1].IsBitSet(1),
                report[1].IsBitSet(2)
            };

            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)report[9]
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public Vector2 Tilt { set; get; }
        public bool Eraser { set; get; }
    }
}
