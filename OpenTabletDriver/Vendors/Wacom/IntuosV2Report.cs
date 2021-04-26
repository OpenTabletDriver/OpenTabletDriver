using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct IntuosV2TabletReport : ITabletReport, IProximityReport, ITiltReport
    {
        public IntuosV2TabletReport(byte[] report)
        {
            Raw = report;
            ReportID = report[1] switch
            {
                0x80 => 0u, // 0x80 is pen out of range report,
                0xC2 => 0u, // <- should fix the GD 0405 U,
                0x20 => 1u, // this should be excluded from IntuosHT2
                _ => 2u     // everything else should have position data.
            };
            Position = new Vector2
            {
                X = (report[3] | report[2] << 8) << 1 | ((report[9] >> 1) & 1),
                Y = (report[5] | report[4] << 8) << 1 | (report[9] & 1)
            };
            Tilt = new Vector2
            {
                X = (((report[7] << 1) & 0x7E) | (report[8] >> 7)) - 64,
                Y = (report[8] & 0x7F) - 64
            };
            Pressure = (uint)((report[6] << 3) | ((report[7] & 0xC0) >> 5) | (report[1] & 1));

            var penByte = report[1];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
            NearProximity = (report[1] & (1 << 6)) != 0;
            HoverDistance = (uint)report[9] >> 2;
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}