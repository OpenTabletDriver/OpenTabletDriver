using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkA15TabletReport : ITabletReport
    {
        public VeikkA15TabletReport(byte[] report)
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
                (report[2] & (1 << 1)) != 0,
                (report[2] & (1 << 2)) != 0
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
