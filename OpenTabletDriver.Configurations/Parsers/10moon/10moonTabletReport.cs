using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public struct TenMoonTabletReport : ITabletReport
    {
        public TenMoonTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = report[1] << 8 | report[2],
                Y = Math.Max((short)(report[3] << 8 | report[4]), (short)0)
            };

            var buttonPressed = (report[9] & 6) != 0;
            var prePressure = report[5] << 8 | report[6];
            Pressure = (uint)(0x0672 - (prePressure - (buttonPressed ? 50 : 0)));

            PenButtons = new bool[]
            {
                report[9].IsBitSet(2),
                (report[9] & 6) == 6
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
