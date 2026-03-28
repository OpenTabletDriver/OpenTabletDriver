using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Dial report for Waltop Sirius Battery Free Tablet in tablet mode.
    /// Report ID 0x0A, subreport IDs 0x02/0x03/0x04. 8 bytes.
    ///
    /// The firmware sends different subreport IDs depending on the active dial mode
    /// (selected via frame buttons), but the byte layout is identical:
    ///   0x02 = Scroll mode
    ///   0x03 = Zoom/volume mode (subfunction switch OFF)
    ///   0x04 = Zoom/volume mode (subfunction switch ON)
    ///
    /// Limitation: all three subreports map to the same wheel report type, so the
    /// Scroll/Multimedia mode toggle on the tablet has no effect — the user-configured
    /// wheel bindings apply regardless of mode. Direction mode (Report 0x0D) is
    /// handled separately by WaltopSiriusKeyDialReport and IS distinguishable.
    ///
    /// Byte layout:
    ///   data[0] = Report ID (0x0A)
    ///   data[1] = Subreport ID (0x02, 0x03, or 0x04)
    ///   data[2] = Rotation delta: -1 (CCW), 0 (none), +1 (CW)
    ///   data[3-5] = Padding (zero)
    ///   data[6] = Right wheel finger position (0=no finger, 1-8 clockwise from 12 o'clock)
    ///   data[7] = Left wheel finger position (0=no finger, 1-8 clockwise from 12 o'clock)
    ///
    /// The rotation delta (byte 2) does not indicate which wheel produced it.
    /// The active wheel is inferred from the absolute finger position bytes.
    /// </summary>
    public struct WaltopSiriusDialReport : IAbsoluteWheelReport
    {
        public WaltopSiriusDialReport(byte[] data)
        {
            Raw = data;

            byte leftPos = data[7];
            byte rightPos = data[6];

            AnalogPositions =
            [
                leftPos != 0 ? leftPos - 1u : null,
                rightPos != 0 ? rightPos - 1u : null
            ];
        }

        public byte[] Raw { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
