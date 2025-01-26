using System;
using System.Numerics;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public struct TenMoonTabletReport : ITabletReport, IAuxReport
    {
        public TenMoonTabletReport(byte[] report)
        {
            Raw = report;

            Position = new Vector2
            {
                X = report[1] << 8 | report[2],
                Y = Math.Max((short)(report[3] << 8 | report[4]), (short)0)
            };

            var buttonPressed = (report[9] & 6) != 0;
            var prePressure = report[5] << 8 | report[6];
            Pressure = (uint)(1800 - (prePressure - (buttonPressed ? 50 : 0)));

            PenButtons = new bool[]
            {
                (report[9] & 6) == 4,
                (report[9] & 6) == 6
            };

            AuxButtons = new bool[]
            {
                !report[12].IsBitSet(1), // First
                !report[11].IsBitSet(7), // Second
                !report[11].IsBitSet(6), // Third
                !report[11].IsBitSet(5), // Fourth
                !report[11].IsBitSet(4), // Fifth
                !report[11].IsBitSet(3), // Sixth
                !report[12].IsBitSet(4), // Seventh
                !report[12].IsBitSet(0), // Eightth
                !report[12].IsBitSet(5), // Nineth
                !report[11].IsBitSet(0), // Tenth
                !report[11].IsBitSet(1), // Eleventh
                !report[11].IsBitSet(2) // Twelveth
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
