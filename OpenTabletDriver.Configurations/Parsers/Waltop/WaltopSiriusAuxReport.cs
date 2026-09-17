using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Frame button report for Waltop Sirius Battery Free Tablet in tablet mode.
    /// Report ID 0x0A, subreport ID 0x0E.
    ///
    /// Byte layout (from DIGImend reverse engineering):
    ///   data[0] = Report ID (0x0A)
    ///   data[1] = Subreport ID (0x0E)
    ///   data[2] = bit 0: Left eraser,  bit 1: Left Shift,  bit 2: Left Tab,
    ///             bit 3: Left Alt,     bit 4: Left Ctrl,   bit 5: Right eraser,
    ///             bit 6: Right Shift,  bit 7: Right Tab
    ///   data[3] = bit 0: Right Alt,    bit 1: Right Ctrl,
    ///             bit 2: V/H scroll,   bit 3: Zoom/volume,
    ///             bit 4: KB direction,  bit 5: Dial subfunction switch
    ///
    /// Aux button indices 0-13 in the shared 32-element layout.
    /// </summary>
    public struct WaltopSiriusAuxReport : IAuxReport
    {
        private const int TotalAuxButtons = 32;

        public WaltopSiriusAuxReport(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[TotalAuxButtons];

            AuxButtons[0]  = (report[2] & 0x01) != 0; // Left eraser
            AuxButtons[1]  = (report[2] & 0x02) != 0; // Left Shift
            AuxButtons[2]  = (report[2] & 0x04) != 0; // Left Tab
            AuxButtons[3]  = (report[2] & 0x08) != 0; // Left Alt
            AuxButtons[4]  = (report[2] & 0x10) != 0; // Left Ctrl
            AuxButtons[5]  = (report[2] & 0x20) != 0; // Right eraser
            AuxButtons[6]  = (report[2] & 0x40) != 0; // Right Shift
            AuxButtons[7]  = (report[2] & 0x80) != 0; // Right Tab
            AuxButtons[8]  = (report[3] & 0x01) != 0; // Right Alt
            AuxButtons[9]  = (report[3] & 0x02) != 0; // Right Ctrl
            AuxButtons[10] = (report[3] & 0x04) != 0; // V/H scroll select
            AuxButtons[11] = (report[3] & 0x08) != 0; // Zoom/volume select
            AuxButtons[12] = (report[3] & 0x10) != 0; // KB direction select
            AuxButtons[13] = (report[3] & 0x20) != 0; // Dial subfunction switch
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
