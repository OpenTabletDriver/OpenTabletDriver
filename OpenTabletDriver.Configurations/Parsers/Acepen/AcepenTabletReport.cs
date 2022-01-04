using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Acepen
{
    public struct AcepenTabletReport : ITabletReport, ITiltReport
    {
        public AcepenTabletReport(byte[] report)
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
                report[2].IsBitSet(1),
                report[2].IsBitSet(2)
            };

            Tilt = new Vector2
            {
                X = report[9],
                Y = report[10]
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public Vector2 Tilt { set; get; }
    }
}
