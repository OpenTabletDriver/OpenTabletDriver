using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Huion
{
    public struct GianoReport : ITabletReport, ITiltReport
    {
        internal GianoReport(byte[] report)
        {
            Raw = report;

            ReportID = (uint)report[1] >> 1;
            Position = new Vector2
            {
                X = BitConverter.ToUInt16(report, 2) | ((report[8] & 1) << 16),
                Y = BitConverter.ToUInt16(report, 4)
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10],
                Y = (sbyte)report[11]
            };
            Pressure = BitConverter.ToUInt16(report, 6);
            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0
            };
        }

        public byte[] Raw { private set; get; }
        public uint ReportID { private set; get; }
        public Vector2 Position { private set; get; }
        public Vector2 Tilt { private set; get; }
        public uint Pressure { private set; get; }
        public bool[] PenButtons { private set; get; }
    }
}
