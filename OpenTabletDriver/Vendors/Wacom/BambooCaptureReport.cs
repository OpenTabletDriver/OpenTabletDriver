using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct BambooCaptureReport : ITabletReport, IAuxReport, IEraserReport
    {
        public BambooCaptureReport(byte[] report)
        {
            Raw = report;

            ReportID = (uint)report[1] >> 1;
            Position = new Vector2
            {
                X = BitConverter.ToUInt16(report, 2),
                Y = BitConverter.ToUInt16(report, 4)
            };
            Pressure = (uint)(report[6] | ((report[7] & 0x01) << 8));
            Eraser = (report[1] & (1 << 3)) != 0;

            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0
            };
            AuxButtons = new bool[]
            {
                (report[7] & (1 << 3)) != 0,
                (report[7] & (1 << 4)) != 0,
                (report[7] & (1 << 5)) != 0,
                (report[7] & (1 << 6)) != 0
            };
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
