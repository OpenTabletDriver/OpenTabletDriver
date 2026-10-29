using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public struct Intuos4MouseReport : IMouseReport, IProximityReport
    {
        public Intuos4MouseReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = (report[3] | report[2] << 8) << 1 | report[9] >> 1 & 1,
                Y = (report[5] | report[4] << 8) << 1 | report[9] & 1
            };
            MouseButtons =
            [
                report[6].IsBitSet(0), // primary
                report[6].IsBitSet(2), // secondary
                report[6].IsBitSet(1), // middle
                report[6].IsBitSet(3), // forward
                report[6].IsBitSet(4), // backward
            ];
            Scroll = new Vector2
            {
                Y = report[7].IsBitSet(7) ? 1 : report[7].IsBitSet(6) ? -1 : 0
            };

            NearProximity = report[1].IsBitSet(6);
            HoverDistance = (uint)report[9] >> 2;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public bool[] MouseButtons { set; get; }
        public Vector2 Scroll { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
