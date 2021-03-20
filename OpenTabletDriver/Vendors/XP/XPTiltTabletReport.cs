using System.Collections.Specialized;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP
{
    public struct XPTiltTabletReport : ITabletReport, ITiltReport, IEraserReport
    {
        internal XPTiltTabletReport(byte[] report)
        {
            Raw = report;

            var bitVector = new BitVector32(report[1]);
            ReportID = (uint)report[1] >> 1;
            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)report[9]
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);
            Eraser = bitVector[1 << 3];
            PenButtons = new bool[]
            {
                bitVector[1 << 1],
                bitVector[1 << 2]
            };
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
