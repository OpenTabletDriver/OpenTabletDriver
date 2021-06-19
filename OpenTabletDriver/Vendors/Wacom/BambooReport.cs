using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public struct BambooReport : ITabletReport, IAuxReport, IEraserReport
    {
        public BambooReport(byte[] report)
        {
            Raw = report;

            ReportID = (uint)report[1] >> 1;
            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            Pressure = (uint)(report[6] | ((report[7] & 0x01) << 8));
            Eraser = report[1].IsBitSet(5);

            var penByte = report[1];
            PenButtons = new bool[]
            {
                penByte.IsBitSet(1),
                penByte.IsBitSet(2)
            };

            var auxByte = report[7];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(3),
                auxByte.IsBitSet(4),
                auxByte.IsBitSet(5),
                auxByte.IsBitSet(6),
            };
        }

        public byte[] Raw { set; get; }
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool Eraser { set; get; }
    }
}
