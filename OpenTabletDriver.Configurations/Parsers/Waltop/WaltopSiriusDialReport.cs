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
    /// Each subreport ID and subfunction switch combination maps to a different pair
    /// of wheel indices, allowing users to assign separate wheel bindings per mode:
    ///   0x02 + subfunc OFF (Scroll):     positions at indices [0, 1]
    ///   0x02 + subfunc ON  (Scroll alt): positions at indices [2, 3]
    ///   0x03 (Zoom/volume, subfunc OFF): positions at indices [4, 5]
    ///   0x04 (Zoom/volume, subfunc ON):  positions at indices [6, 7]
    /// Raw positions (1-8) are converted to 0-based (0-7); 0 means no finger (null).
    /// Inactive indices are null, which resets the corresponding WheelBindings.
    /// Direction mode (Report 0x0D) is handled separately by WaltopSiriusKeyDialReport.
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
        public WaltopSiriusDialReport(byte[] data, bool subfunctionSwitch)
        {
            Raw = data;

            int offset = data[1] switch
            {
                0x02 => subfunctionSwitch ? 2 : 0,
                0x03 => 4,
                0x04 => 6,
                _ => 0
            };

            AnalogPositions = new uint?[8];
            AnalogPositions[offset] = RawToPosition(data[7]);
            AnalogPositions[offset + 1] = RawToPosition(data[6]);
        }

        private static uint? RawToPosition(byte raw)
        {
            return raw == 0 ? null : (uint)(raw - 1);
        }

        public byte[] Raw { set; get; }
        public uint?[] AnalogPositions { set; get; }
    }
}
