using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ISKN
{
    /// <summary>
    /// Pen sample (frame type 0x05). Payload, little endian:
    ///   int16 x, int16 y, int16 z (height), uint16 timestamp, uint16 tilt, int16 azimuth, byte flags.
    /// Units are 1/100 mm and 1/100 degree. The tablet's x axis runs bottom to top and its
    /// y axis left to right, so axes are swapped and re-based here.
    /// </summary>
    public struct SlateTabletReport : ITabletReport, ITiltReport, IProximityReport
    {
        // Measured on a TS2EI: left edge y≈3850, right edge y≈25800, bottom x≈0, top x≈17750.
        private const int MinY = 3850;
        private const int MaxX = 17750;

        public SlateTabletReport(byte[] report)
        {
            Raw = report;

            var rawX = Unsafe.ReadUnaligned<short>(ref report[5]);
            var rawY = Unsafe.ReadUnaligned<short>(ref report[7]);
            var height = Unsafe.ReadUnaligned<short>(ref report[9]);
            var tilt = Unsafe.ReadUnaligned<ushort>(ref report[13]) / 100f;
            var azimuth = Unsafe.ReadUnaligned<short>(ref report[15]) / 100f * MathF.PI / 180f;
            var contact = (report[17] & 0x01) != 0;

            Position = new Vector2
            {
                X = Math.Clamp(rawY - MinY, 0, 21950),
                Y = Math.Clamp(MaxX - rawX, 0, MaxX)
            };
            // Azimuth 0 points to the bottom of the tablet and grows clockwise (verified on device):
            // X is positive when leaning right, Y is positive when leaning towards the user.
            Tilt = new Vector2
            {
                X = -tilt * MathF.Sin(azimuth),
                Y = tilt * MathF.Cos(azimuth)
            };
            Pressure = contact ? 1u : 0u;
            PenButtons = Array.Empty<bool>();
            NearProximity = true;
            HoverDistance = (uint)Math.Max((int)height, 0);
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
