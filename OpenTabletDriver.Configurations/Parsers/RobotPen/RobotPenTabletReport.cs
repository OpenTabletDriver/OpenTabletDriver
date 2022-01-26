using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.RobotPen
{
    public struct RobotPenTabletReport : ITabletReport
    {
        public RobotPenTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[6]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[8])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[10]);


            PenButtons = new bool[]
            {
                (report[11] & (1 << 1)) != 0
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
