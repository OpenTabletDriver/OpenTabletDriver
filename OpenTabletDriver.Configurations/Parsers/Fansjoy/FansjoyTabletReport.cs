using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Fansjoy
{
    public struct FansjoyTabletReport : ITabletReport
    {
        public FansjoyTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = (report[5] << 8 | report[4]),
                Y = 16800 - (report[3] << 8 | report[2])
            };

            Pressure = (uint)(report[7] << 8 | report [6]);

            PenButtons = new bool[]
            {
                report[1].IsBitSet(2),
                report[1].IsBitSet(1)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
