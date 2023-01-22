using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public struct TenMoonTabletReport : ITabletReport
    {
        public TenMoonTabletReport(byte[] report)
        {
            Raw = report;

            var x = report[1] << 8 | report[2];
            var y = report[3] << 8 | report[4];

            // don't ask me why it's like this
            if ((y & 0x8000) != 0)
                y = (ushort)(0x8F - (ushort)(y & 0x7FFF));
            else
                y += 0x8F;

            Position = new Vector2(x, y);

            ushort prePressure = (ushort)(report[5] << 8 | report[6]);
            ushort calibratedMax = (ushort)(report[7] << 8 | report[8]);

            ushort pressure = (ushort)(calibratedMax - prePressure);
            if ((pressure & 0x8000) != 0)
                pressure = 0;

            Pressure = pressure;

            PenButtons = new bool[]
            {
                (report[9] & 0b110) == 0b100,
                (report[9] & 0b110) == 0b110
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
