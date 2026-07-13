using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Bosto
{
    public struct Bosto13HDTabletReport : ITabletReport, IProximityReport
    {
         // tablet raw limits
        private const float MAX_X = 17920f;
        private const float MAX_Y = 10240f;

        // default calibration
        private const float X_OFFSET = 20300f;
        private const float Y_OFFSET = 11200f;

        private const float X_STRETCH = 3.25f;
        private const float Y_STRETCH = 3.115f;


        // Centers
        private const float CX = MAX_X / 2f;
        private const float CY = MAX_Y / 2f;

        public Bosto13HDTabletReport(byte[] report)
        {
            Raw = report;

            ushort x_raw = Unsafe.ReadUnaligned<ushort>(ref report[2]);
            ushort y_raw = Unsafe.ReadUnaligned<ushort>(ref report[4]);

            Position = new Vector2
            {
                X = MathF.Round((x_raw - CX) * X_STRETCH + CX + X_OFFSET),
                Y = MathF.Round((y_raw - CY) * Y_STRETCH + CY + Y_OFFSET)
            };

            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            PenButtons =
            [
                report[1].IsBitSet(5),
                report[1].IsBitSet(1),
            ];

            NearProximity = report[1].IsBitSet(4);
            HoverDistance = 0;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
