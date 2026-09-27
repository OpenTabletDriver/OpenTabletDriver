using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public struct IntuosV1RotationReport : ITabletReport, IProximityReport, ITiltReport, IRotationReport
    {
        public IntuosV1RotationReport(byte[] report, ref uint _prevPressure, ref Vector2 _prevTilt, ref int _prevRotation, ref bool[] _prevPenButtons)
        {
            Raw = report;

            Position = new Vector2
            {
                X = (report[3] | report[2] << 8) << 1 | ((report[9] >> 1) & 1),
                Y = (report[5] | report[4] << 8) << 1 | (report[9] & 1)
            };
            Tilt = _prevTilt;
            Pressure = _prevPressure;

            PenButtons = _prevPenButtons;
            NearProximity = report[1].IsBitSet(6);
            HoverDistance = (uint)report[9];

            Rotation = (report[6] << 2) | ((report[7] & 0xC0) >> 6);
            if (!report[7].IsBitSet(5)) Rotation *= -1;

            _prevRotation = Rotation;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
        public int Rotation { set; get; }
    }
}
