using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkTiltTabletReport : ITabletReport, ITiltReport
    {
        public VeikkTiltTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]) | (report[5] << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[6]) | (report[8] << 16)
            };
            Tilt = new Vector2
            {
                X = unchecked((sbyte)report[11]),
                Y = unchecked((sbyte)report[12])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[9]);
            PenButtons = new bool[]
            {
                (report[2] & (1 << 1)) != 0,
                (report[2] & (1 << 2)) != 0
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}

