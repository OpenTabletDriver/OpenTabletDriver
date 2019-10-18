using System;
using System.Linq;

namespace TabletDriverLib.Class
{
    public struct TabletReport
    {
        internal TabletReport(byte[] report)
        {
            Raw = report;
            Lift = (uint) report[1] / report[0];
            var x = BitConverter.ToUInt16(report, 2);
            var y = BitConverter.ToUInt16(report, 4);
            Position = new Point(x,y);
            Pressure = BitConverter.ToUInt16(report, 6);
        }

        public byte[] Raw { private set; get; }
        public uint Lift { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }

        public override string ToString() => ToString(false);

        public string ToString(bool raw)
        {
            if (raw)
                return BitConverter.ToString(Raw).Replace('-', ' ');
            else
                return $"Lift:{Lift}, Position:[{Position}], Pressure:{Pressure}";
        }
    }
}