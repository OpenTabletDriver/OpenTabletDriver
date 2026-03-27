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
    /// </summary>
    public struct WaltopSiriusAuxReport : IAuxReport
    {
        public WaltopSiriusAuxReport(byte[] report)
        {
            Raw = report;

            AuxButtons = new bool[]
            {
                (report[2] & 0x01) != 0, // Left eraser
                (report[2] & 0x02) != 0, // Left Shift
                (report[2] & 0x04) != 0, // Left Tab
                (report[2] & 0x08) != 0, // Left Alt
                (report[2] & 0x10) != 0, // Left Ctrl
                (report[2] & 0x20) != 0, // Right eraser
                (report[2] & 0x40) != 0, // Right Shift
                (report[2] & 0x80) != 0, // Right Tab
                (report[3] & 0x01) != 0, // Right Alt
                (report[3] & 0x02) != 0, // Right Ctrl
                (report[3] & 0x04) != 0, // V/H scroll select
                (report[3] & 0x08) != 0, // Zoom/volume select
                (report[3] & 0x10) != 0, // KB direction select
                (report[3] & 0x20) != 0, // Dial subfunction switch
            };
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
