using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Huion
{
    public struct GianoReport : ITabletReport, ITiltReport
    {
        internal GianoReport(byte[] report)
        {
            Raw = report;

            ReportID = (uint)report[1] >> 1;
            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]) | ((report[8] & 1) << 16),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Tilt = new Vector2
            {
                X = (sbyte)report[10],
                Y = (sbyte)report[11]
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            var penByte = report[1];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
