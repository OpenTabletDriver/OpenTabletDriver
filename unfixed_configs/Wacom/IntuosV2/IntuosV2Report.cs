using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2
{
    public struct IntuosV2Report : ITabletReport, IProximityReport, ITiltReport, IEraserReport, IRotationReport
    {
        public IntuosV2Report(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]) | (report[4] << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[5]) | (report[7] << 16)
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10],
                Y = (sbyte)report[11]
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[8]);

            var penByte = report[1];
            Eraser = penByte.IsBitSet(4);
            PenButtons =
            [
                penByte.IsBitSet(1),
                penByte.IsBitSet(2),
            ];
            NearProximity = report[1].IsBitSet(5);
            HoverDistance = report[16];

            Rotation = Unsafe.ReadUnaligned<short>(ref report[12]);
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
        public int Rotation { set; get; }
    }
}
