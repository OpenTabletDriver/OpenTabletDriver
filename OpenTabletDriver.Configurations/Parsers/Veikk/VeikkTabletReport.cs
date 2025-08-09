using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkTabletReport : ITabletReport
    {
        public VeikkTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]) | (report[5] << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[6]) | (report[8] << 16)
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
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
