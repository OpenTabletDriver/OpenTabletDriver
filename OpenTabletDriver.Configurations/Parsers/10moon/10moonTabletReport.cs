using System;
using System.Numerics;
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

            var buttonPressed = (report[9] & 6) is 4 or 6;
            var prePressure = report[5] << 8 | report[6];
            var correctedPressure = 0x05DC - (prePressure - (buttonPressed ? 50 : 0));

            // Byte 10: 0x0F = no ar, 0x0A = bem perto, 0x05 = encostada na mesa
            var touching = report[10] == 0x05;

            const int deadZone = 20;
            const int measuredMax = 790;

            if (!touching || correctedPressure <= deadZone)
            {
                Pressure = 0;
            }
            else
            {
                var scaledPressure =
                    (long)(correctedPressure - deadZone) * 8191 /
                    (measuredMax - deadZone);

                Pressure = (uint)Math.Clamp(scaledPressure, 0, 8191);
            }

            // Byte 9: 02 = nenhum, 04 = botão 1, 06 = botão 2
            PenButtons =
            [
                (report[9] & 6) == 4,
                (report[9] & 6) == 6,
            ];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
