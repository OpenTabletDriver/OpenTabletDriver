using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Border location report for Waltop Sirius Battery Free Tablet.
    /// Report ID 0x05, 8 bytes. Emitted when the pen is near the tablet edge.
    ///
    /// Byte layout:
    ///   data[1] bits 0-1: proximity, bit 2: tip, bit 3: lower button, bit 4: upper button
    ///   data[2]: border (1=bottom, 2=top, 4=left, 8=right)
    ///   data[3]: position along border (horizontal: 0x01-0x28, vertical: 0x01-0x18)
    ///
    /// The top border contains 10 virtual buttons (K1-K10), each spanning 4 positions
    /// with 1-position gaps: K1=0x01-0x04, K2=0x05-0x08, ... K10=0x25-0x28.
    /// A button is considered pressed when the tip bit is set (pen tapping, not just hovering).
    ///
    /// Aux button indices 22-31 in the shared 32-element layout.
    /// </summary>
    public struct WaltopSiriusBorderReport : IAuxReport
    {
        private const int TotalAuxButtons = 32;
        private const int VirtualButtonOffset = 22;
        private const int VirtualButtonCount = 10;
        private const byte TopBorder = 2;

        public WaltopSiriusBorderReport(byte[] data)
        {
            Raw = data;
            AuxButtons = new bool[TotalAuxButtons];

            bool tip = (data[1] & 0x04) != 0;
            byte border = data[2];
            byte position = data[3];

            if (tip && border == TopBorder && position >= 1 && position <= 0x28)
            {
                int buttonIndex = (position - 1) / 4;
                if (buttonIndex < VirtualButtonCount)
                    AuxButtons[VirtualButtonOffset + buttonIndex] = true;
            }
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
