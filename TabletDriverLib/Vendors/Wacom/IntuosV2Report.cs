using System;
using TabletDriverLib.Component;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public struct IntuosV2TabletReport : ITabletReport
    {
        public IntuosV2TabletReport(byte[] report)
        {
            Raw = report;

            Lift = (uint)report[9] >> 2;

            var x = (report[2] * 0x100 + report[3]) << 1;
            var y = (report[4] * 0x100 + report[5]) << 1;
            Position = new Point(x, y);
            Pressure = (uint)(report[6] << 3);
            
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

        public override string ToString() => ToString(false);

        public string ToString(bool raw)
        {
            if (raw)
                return BitConverter.ToString(Raw).Replace('-', ' ');
            else
                return $"Lift:{Lift}, Position:[{Position}], Pressure:{Pressure}, PenButtons:[{String.Join(" ", PenButtons)}]";
        }
    }
}