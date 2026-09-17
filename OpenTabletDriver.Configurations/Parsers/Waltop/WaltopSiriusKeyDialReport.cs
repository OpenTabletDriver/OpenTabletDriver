using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Keyboard-direction dial report for Waltop Sirius Battery Free Tablet.
    /// Report ID 0x0D, 8 bytes. Active when the "Keyboard direction" frame button is selected.
    ///
    /// Only data[3] carries the payload — a HID keyboard usage code:
    ///
    ///   Subfunction switch OFF:
    ///     0x52 = Up,    0x51 = Down
    ///     0x4F = Right, 0x50 = Left
    ///
    ///   Subfunction switch ON:
    ///     0x4B = Page Up,   0x4E = Page Down
    ///     0x4A = Home,      0x4D = End
    ///
    /// Aux button indices 14-21 in the shared 32-element layout:
    ///   [14]=Up, [15]=Down, [16]=Left, [17]=Right,
    ///   [18]=PageUp, [19]=PageDown, [20]=Home, [21]=End.
    /// Only one button is active at a time; all false when data[3] == 0 (release).
    /// </summary>
    public struct WaltopSiriusKeyDialReport : IAuxReport
    {
        private const int TotalAuxButtons = 32;
        private const int KeyDialOffset = 14;

        public WaltopSiriusKeyDialReport(byte[] data)
        {
            Raw = data;
            AuxButtons = new bool[TotalAuxButtons];

            byte key = data[3];

            int index = key switch
            {
                0x52 => KeyDialOffset + 0, // Up
                0x51 => KeyDialOffset + 1, // Down
                0x50 => KeyDialOffset + 2, // Left
                0x4F => KeyDialOffset + 3, // Right
                0x4B => KeyDialOffset + 4, // Page Up
                0x4E => KeyDialOffset + 5, // Page Down
                0x4A => KeyDialOffset + 6, // Home
                0x4D => KeyDialOffset + 7, // End
                _ => -1
            };

            if (index >= 0)
                AuxButtons[index] = true;
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
