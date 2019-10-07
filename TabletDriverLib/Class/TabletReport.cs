using System;
using System.Linq;

namespace TabletDriverLib.Class
{
    public class TabletReport
    {
        private TabletReport()
        {
        }

        internal TabletReport(byte[] report)
        {
            InRange = report[0] == 0x02;
            Pressure = report[7];
            var x = BitConverter.ToUInt16(report, 2);
            var y = BitConverter.ToUInt16(report, 4);
            Position = new Point(x,y);
            Pressure = BitConverter.ToUInt16(report, 6);
        }

        public bool InRange { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }
    }
}