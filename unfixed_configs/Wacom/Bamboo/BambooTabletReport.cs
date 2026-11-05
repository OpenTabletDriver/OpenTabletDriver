using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Bamboo
{
    public struct BambooTabletReport : ITabletReport, IAuxReport, IEraserReport, IProximityReport, IAbsoluteWheelReport
    {
        public BambooTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            Pressure = report[1].IsBitSet(0) ? (uint)(report[6] | ((report[7] & 0x03) << 8)) : 0;
            Eraser = report[1].IsBitSet(5);

            PenButtons =
            [
                report[1].IsBitSet(1),
                report[1].IsBitSet(2),
            ];

            AuxButtons =
            [
                report[7].IsBitSet(3),
                report[7].IsBitSet(4),
                report[7].IsBitSet(5),
                report[7].IsBitSet(6),
            ];

            HoverDistance = 0;
            NearProximity = report[1].IsBitSet(7);

            AnalogPositions = [report[8].IsBitSet(7) ? (uint)(report[8] & 0x7f) : null];
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool Eraser { set; get; }
        public uint HoverDistance { set; get; }
        public bool NearProximity { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
