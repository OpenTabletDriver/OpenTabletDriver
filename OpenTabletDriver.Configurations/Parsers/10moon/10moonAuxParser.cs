using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    public struct TenMoonAuxReport : IAuxReport
    {
        public TenMoonAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                report[12] == 0x31, // First
                report[12] == 0x33 && !report[11].IsBitSet(7), // Second
                report[12] == 0x33 && !report[11].IsBitSet(6), // Third
                report[12] == 0x33 && !report[11].IsBitSet(5), // Fourth
                report[12] == 0x33 && !report[11].IsBitSet(4), // Fifth
                report[12] == 0x33 && !report[11].IsBitSet(3), // Sixth
                report[12] == 0x23, // Seventh
                report[12] == 0x32, // Eightth
                report[12] == 0x13, // Nineth
                report[12] == 0x33 && !report[11].IsBitSet(0), // Tenth
                report[12] == 0x33 && !report[11].IsBitSet(1), // Eleventh
                report[12] == 0x33 && !report[11].IsBitSet(2) // Twelveth
            };
        }

        public bool[] AuxButtons { set; get; }
        public byte[] Raw { set; get; }
    }
}
