using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct IntuosV3Report : ITabletReport, IProximityReport, ITiltReport, IEraserReport
    {
        public IntuosV3Report(byte[] report)
        {
            Raw = report;
            ReportID = report[1] != 0 ? report[0] : 0u;

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
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
            NearProximity = report[1].IsBitSet(5);
            HoverDistance = report[16];
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
        public bool NearProximity { set; get; }
        public uint HoverDistance { set; get; }
    }
}
