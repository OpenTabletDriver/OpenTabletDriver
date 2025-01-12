using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Graphire
{
    public struct GraphireTabletReport : ITabletReport, IAuxReport, IEraserReport, IConfidenceReport
    {
        public GraphireTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            Pressure = report[1].IsBitSet(0) ? (uint)(report[6] | ((report[7] & 0x03) << 8)) : 0;
            Eraser = report[1].IsBitSet(5);

            PenButtons = new bool[]
            {
                report[1].IsBitSet(1),
                report[1].IsBitSet(2)
            };

            var auxByte = report[7];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            };

            // wheel = report[7][5:3]

            HighConfidence = report[1].IsBitSet(7);
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
        public bool[] AuxButtons { set; get; }
        public bool Eraser { set; get; }
        public bool HighConfidence { set; get; }
    }
}
