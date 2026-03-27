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
    /// Byte layout (from DIGImend reverse engineering):
    ///   data[0]    = Report ID (0x02)
    ///   data[1:2]  = X position (uint16 LE, 0-40000)
    ///   data[3:4]  = Y position (uint16 LE, 0-24000)
    ///   data[5]    = Flags:
    ///                  bits 0-1: Proximity (non-zero = in range)
    ///                  bit 2:    Tip button
    ///                  bit 3:    Lower side button
    ///                  bit 4:    Upper side button
    ///                  bits 5-7: Padding
    ///   data[6:7]  = Tip pressure (uint16 LE, 0-1023)
    ///   data[8]    = X tilt
    ///   data[9]    = Y tilt
    /// </summary>
    public struct WaltopSiriusTabletReport : ITabletReport, ITiltReport
    {
        public WaltopSiriusTabletReport(byte[] report, byte debouncedFlags)
        {
            Raw = report;

            bool tipSwitch = (debouncedFlags & 0x04) != 0;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[1]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[3])
            };

            // Only report pressure when tip is touching; device reports
            // a baseline pressure during hover that must be zeroed.
            Pressure = tipSwitch ? Unsafe.ReadUnaligned<ushort>(ref report[6]) : 0u;

            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)(-report[9])
            };

            // The pen has two physical side buttons but they share a common
            // electrical contact — the hardware alternates between bit 3 and
            // bit 4 for either button. Merge into a single barrel button,
            // matching the Linux hid-waltop.c approach: (data[1] & 0xF) > 1.
            PenButtons = new bool[]
            {
                (debouncedFlags & 0x18) != 0, // bits 3-4 = barrel button (either side button)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
