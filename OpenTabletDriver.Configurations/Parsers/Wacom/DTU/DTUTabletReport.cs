using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.DTU
{
    public struct DTUTabletReport : ITabletReport
    {
        public DTUTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = (report[3] << 8) | report[4],
                Y = (report[5] << 8) | report[6]
            };
            Pressure = ((uint)(report[1] & 0x1) << 8 ) | report[2];

            PenButtons = new bool[]
            {
            };

        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
