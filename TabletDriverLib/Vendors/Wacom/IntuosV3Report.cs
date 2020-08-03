using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class IntuosV3Report : ITabletReport
    {
        public IntuosV3Report(byte[] report)
        {
            Raw = report;

            ReportID = report[0];
            var x = (report[2] | (report[3] << 8) | (report[4] << 16));
            var y = (report[5] | (report[6] << 8) | (report[7] << 16));
            Position = new Point(x, y);
            Pressure = (uint)(report[8] | (report[9] << 8));

            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0
            };
        }
        
        public byte[] Raw { private set; get; }
        public uint ReportID { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }
        public bool[] PenButtons { private set; get; }
    }
}