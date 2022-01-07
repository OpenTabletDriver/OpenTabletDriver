using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos
{
    public struct IntuosTabletReport : ITabletReport, IEraserReport, IProximityReport
    {
        public IntuosTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            PenButtons = new bool[]
            {
                report[1].IsBitSet(1),
                report[1].IsBitSet(2)
            };
            Eraser = report[1].IsBitSet(3);
            NearProximity = report[1].IsBitSet(7);
            HoverDistance = (uint)report[8] >> 2;
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool Eraser { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
