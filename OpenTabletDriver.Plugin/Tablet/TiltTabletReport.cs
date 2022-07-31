using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver.Plugin.Tablet
{
    public struct TiltTabletReport : ITabletReport, ITiltReport
    {
        public TiltTabletReport(byte[] report, bool invertTiltX, bool invertTiltY)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Tilt = new Vector2
            {
                X = (sbyte)(invertTiltX ? -1 : 1) * (sbyte)report[10],
                Y = (sbyte)(invertTiltY ? -1 : 1) * (sbyte)report[11]
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);

            var penByte = report[1];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };
        }

        public TiltTabletReport(byte[] report) : this(report, false, false) { }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public Vector2 Tilt { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
