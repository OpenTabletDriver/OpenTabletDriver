using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV3
{
    public struct IntuosV3Report : ITabletReport, IHoverReport, IConfidenceReport, ITiltReport
    {
        public IntuosV3Report(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[3]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5])
            };

            Tilt = new Vector2
            {
                X = report[9].IsBitSet(7) ? report[9] - 0xFF : report[9],
                Y = report[11].IsBitSet(7) ? report[11] - 0xFF : report[11]
            };

            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[7]);

            var penByte = report[2];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
            HighConfidence = report[2].IsBitSet(6);
            HoverDistance = report[13];
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
