using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Graphire
{
    public struct GraphireMouseReport : IMouseReport, IAuxReport
    {
        public GraphireMouseReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            Scroll = new Vector2
            {
                Y = report[7].IsBitSet(1) ? -(report[7] & 1) : report[7] & 1
            };

            MouseButtons = new bool[]
            {
                report[1].IsBitSet(0), // LEFT
                report[1].IsBitSet(1), // RIGHT
                report[1].IsBitSet(2)  // MIDDLE
            };

            var auxByte = report[7];
            AuxButtons = new bool[]
            {
                auxByte.IsBitSet(6),
                auxByte.IsBitSet(7),
            };

            // wheel = report[7][5:3]

            // Models using Graphire WACOM_MO protocol also reports mouse hover distance
        }

        public byte[] Raw { get; set; }
        public bool[] MouseButtons { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Scroll { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
