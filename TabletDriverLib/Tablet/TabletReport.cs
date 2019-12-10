using System;
using TabletDriverLib.Component;

namespace TabletDriverLib.Tablet
{
    public struct TabletReport : ITabletReport
    {
        internal TabletReport(byte[] report)
        {
            Raw = report;

            Lift = (uint) report[1] / report[0];
            var x = BitConverter.ToUInt16(report, 2);
            var y = BitConverter.ToUInt16(report, 4);
            Position = new Point(x, y);
            Pressure = BitConverter.ToUInt16(report, 6);

            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0,
                (report[1] & (1 << 3)) != 0,
                (report[1] & (1 << 3)) != 0
            };
            PadButtons = new bool[]
            {
                (report[4] & (1 << 0)) != 0,
                (report[4] & (1 << 1)) != 0,
                (report[4] & (1 << 2)) != 0,
                (report[4] & (1 << 3)) != 0
            };
            IsPadReport = ((report[1] & (1 << 5)) != 0) & ((report[1] & (1 << 6)) != 0);
        }

        public byte[] Raw { private set; get; }
        public uint Lift { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }

        public bool[] PenButtons { private set; get; }
        public bool[] PadButtons { private set; get; }
        public bool IsPadReport { private set; get; }

        public override string ToString() => ToString(true);

        public string ToString(bool raw)
        {
            if (raw)
                return BitConverter.ToString(Raw).Replace('-', ' ');
            else
                return $"Lift:{Lift}, Position:[{Position}], Pressure:{Pressure}, PenButtons:[{String.Join(" ", PenButtons)}], PadButtons:[{String.Join(" ", PadButtons)}], IsPadReport:{IsPadReport}";
        }
    }
}