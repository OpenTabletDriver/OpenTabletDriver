using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Pen report for Waltop Sirius Battery Free Tablet in tablet mode (Report ID 0x02).
    /// Tablet mode is activated by sending feature report {0x10, 0x01} on report ID 0x02,
    /// which enables full 4000 LPI resolution.
    ///
    /// The pen's EMR barrel buttons are physically mutually exclusive (capacitor switching)
    /// but tablet mode encodes them as individual bits that produce noisy alternating signals.
    /// The parser applies SPRT classification to reliably distinguish the two buttons.
    /// </summary>
    public struct WaltopSiriusTabletReport : ITabletReport, ITiltReport
    {
        public WaltopSiriusTabletReport(byte[] report, byte classifiedFlags)
        {
            Raw = report;

            bool tipSwitch = (classifiedFlags & 0x04) != 0;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[3])
            };

            Pressure = tipSwitch ? Unsafe.ReadUnaligned<ushort>(ref report[6]) : 0u;

            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)(-report[9])
            };

            PenButtons = new bool[]
            {
                (classifiedFlags & 0x08) != 0, // barrel (upper physical button)
                (classifiedFlags & 0x10) != 0, // pick (lower physical button)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
