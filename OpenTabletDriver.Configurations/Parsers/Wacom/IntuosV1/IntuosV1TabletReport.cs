using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public struct IntuosV1TabletReport : ITabletReport, IHoverReport, IConfidenceReport, ITiltReport
    {
        public IntuosV1TabletReport(byte[] report, ref uint _prevPressure, ref Vector2 _prevTilt, ref bool[] _prevPenButtons)
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
            HighConfidence = penByte.IsBitSet(6);
            HoverDistance = (uint)report[9];

            _prevPressure = Pressure;
            _prevTilt = Tilt;
            _prevPenButtons = PenButtons;
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
