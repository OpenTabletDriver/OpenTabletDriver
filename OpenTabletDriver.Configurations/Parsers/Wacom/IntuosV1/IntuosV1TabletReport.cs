using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public struct IntuosV1TabletReport : ITabletReport, IProximityReport, ITiltReport
    {
        public IntuosV1TabletReport(byte[] report)
        {
            Raw = report;

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
            NearProximity = report[1].IsBitSet(6);
            HoverDistance = (uint)report[9] >> 2;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
