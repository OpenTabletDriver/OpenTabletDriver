using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Lifetec
{
    public struct LifetecTabletReport : ITabletReport
    {
        public LifetecTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[3])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            PenButtons = new bool[]
            {
                report[5].IsBitSet(3),
                report[5].IsBitSet(4)
            };

            // Tilt is given as azimuth and altitude
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
