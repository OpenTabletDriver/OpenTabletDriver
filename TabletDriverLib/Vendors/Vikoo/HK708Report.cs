using System;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Vendors.Vikoo
{
    public struct HK708Report : ITabletReport
    {
        public HK708Report(byte[] report)
        {
            Raw = report;
            ReportID = (uint)report[0];
            var x = BitConverter.ToUInt16(report, 2);
            var y = BitConverter.ToUInt16(report, 4);
            Position = new Point(x, y);
            Pressure = BitConverter.ToUInt16(report, 6);

            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0, //first buttob
                (report[1] & (1 << 2)) != 0, //second button
            };
        }

        public byte[] Raw { private set; get; }
        public uint ReportID { private set; get; }
        public Point Position { private set; get; }
        public uint Pressure { private set; get; }
        public bool[] PenButtons { private set; get; }
    }
}