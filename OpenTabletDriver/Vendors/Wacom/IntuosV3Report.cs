using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct IntuosV3Report : ITabletReport, IProximityReport, ITiltReport, IEraserReport
    {
        public IntuosV3Report(byte[] report)
        {
            Raw = report;

            if (report.Length < 10)
            {
                // Discard first tablet report or whenever report length is insufficient
                ReportID = 0;
                Position = Vector2.Zero;
                Tilt = Vector2.Zero;
                Pressure = 0;
                Eraser = false;
                PenButtons = new bool[] { false, false };
                NearProximity = false;
                HoverDistance = 0;
                return;
            }

            ReportID = report[0];
            Position = new Vector2
            {
                X = (report[2] | (report[3] << 8) | (report[4] << 16)),
                Y = (report[5] | (report[6] << 8) | (report[7] << 16))
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10],
                Y = (sbyte)report[11]
            };
            Pressure = (uint)(report[8] | (report[9] << 8));
            Eraser = (report[1] & (1 << 4)) != 0;
            PenButtons = new bool[]
            {
                (report[1] & (1 << 1)) != 0,
                (report[1] & (1 << 2)) != 0
            };
            NearProximity = (report[1] & (1 << 5)) != 0;
            HoverDistance = report[16];
        }
        
        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}