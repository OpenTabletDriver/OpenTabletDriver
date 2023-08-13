using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public struct IntuosV1RotationReport : ITabletReport, IHoverReport, IConfidenceReport, ITiltReport
    {
        public IntuosV1RotationReport(byte[] report, ref uint _prevPressure, ref Vector2 _prevTilt, ref bool[] _prevPenButtons)
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
            HighConfidence = report[1].IsBitSet(6);
            HoverDistance = (uint)report[9];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool HighConfidence { set; get; }
        public uint HoverDistance { set; get; }
    }
}
