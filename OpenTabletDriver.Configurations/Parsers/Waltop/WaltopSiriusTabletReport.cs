using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    public struct WaltopSiriusTabletReport : ITabletReport, ITiltReport, IEraserReport
    {
        public WaltopSiriusTabletReport(byte[] report)
        {
            Raw = report;

            // Lower 2 bits of flags are a 2-bit enumeration:
            //   0 = no button, 1 = tip, 2 = barrel (button 1), 3 = tablet pick (button 2)
            int buttonEnum = report[1] & 0x03;
            bool tipSwitch = buttonEnum == 1;

            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };

            // Only report pressure when tip is touching; device reports baseline ~5 on hover.
            // Zero pressure when barrel buttons are pressed (matches Linux hid-waltop.c).
            Pressure = tipSwitch ? Unsafe.ReadUnaligned<ushort>(ref report[6]) : 0u;

            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)(-report[9])
            };

            Eraser = (report[1] & 0x08) != 0;

            PenButtons = new bool[]
            {
                buttonEnum == 2, // barrel switch (side button 1)
                buttonEnum == 3, // tablet pick (side button 2)
            };
        }

        public byte[] Raw { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public Vector2 Tilt { set; get; }
        public bool Eraser { set; get; }
        public bool[] PenButtons { set; get; }
    }
}
