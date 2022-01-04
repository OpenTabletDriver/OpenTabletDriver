using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    public struct WoodPadReport : ITabletReport, ITiltReport
    {
        public WoodPadReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };

            var button = report[9];
            Pressure = button.IsBitSet(2) ? Unsafe.ReadUnaligned<ushort>(ref report[10]) : 0u;

            Tilt = new Vector2
            {
                X = report[12],
                Y = report[13]
            };

            PenButtons = new bool[]
            {
                button.IsBitSet(3),
                button.IsBitSet(4)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { get; set; }
        public bool[] PenButtons { set; get; }
    }
}
