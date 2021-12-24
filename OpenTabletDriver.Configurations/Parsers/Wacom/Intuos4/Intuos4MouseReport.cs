using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4MouseReport : IMouseReport
    {
        public Intuos4MouseReport(byte[] report)
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
                X = (report[3] | report[2] << 8) << 1 | report[9] >> 1 & 1,
                Y = (report[5] | report[4] << 8) << 1 | report[9] & 1
            };
            MouseButtons = new bool[]
            {
                report[6].IsBitSet(0), // primary
                report[6].IsBitSet(2), // secondary
                report[6].IsBitSet(1), // middle
                report[6].IsBitSet(3), // forward
                report[6].IsBitSet(4), // backward
            };
            Scroll = new Vector2
            {
                Y = report[8].IsBitSet(0) ? 1 : report[8].IsBitSet(1) ? -1 : 0
            };
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public bool[] MouseButtons { set; get; }
        public Vector2 Scroll { set; get; }
    }
}