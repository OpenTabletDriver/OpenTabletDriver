using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.PL
{
    public struct PLTabletReport : ITabletReport, IEraserReport
    {
        public PLTabletReport(byte[] report, bool initialEraser)
        {
            Raw = report;

            Position = new Vector2
            {
                X = ((report[1] & 0x03) << 14) + (report[2] << 7) + report[3],
                Y = ((report[4] & 0x03) << 14) + (report[5] << 7) + report[6]
            };

            // pressure is sent in a range from negative to positive
            // report[7].IsBitSet(6) == pressure is negative, flip this bit and read as normal
            Pressure = (uint)(((report[7] ^ 0x40) << 2) + ((report[4] & 0x40) >> 5) + ((report[4] & 0x04) >> 2));

            PenButtons =
            [
                report[4].IsBitSet(4),
                report[4].IsBitSet(5) && !initialEraser
            ];

            Eraser = report[4].IsBitSet(5) && initialEraser;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
