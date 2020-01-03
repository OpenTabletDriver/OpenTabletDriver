using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Tablet
{
    public struct TabletReport : ITabletReport
    {
        internal TabletReport(byte[] report)
        {
            Raw = report;

            Lift = (uint)report[1] >> 1;
            var x = BitConverter.ToUInt16(report, 2);
            var y = BitConverter.ToUInt16(report, 4);
            Position = new Point(x, y);
            Pressure = BitConverter.ToUInt16(report, 6);

            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0,
                (report[1] & (1 << 3)) != 0,
                (report[1] & (1 << 4)) != 0
            };
        }

        public byte[] Raw { private set; get; }
        public uint Lift { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }
        public bool[] PenButtons { private set; get; }

        public override string ToString() => Extensions.GetData(this, false);
    }
}