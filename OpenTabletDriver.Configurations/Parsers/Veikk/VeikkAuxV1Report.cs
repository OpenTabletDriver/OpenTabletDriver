using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkAuxV1Report : IAuxReport
    {
        public VeikkAuxV1Report(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[12];
            for (var i = 2; i < report.Length; i++)
            {
                var buttonId = MapToButton(report[1], report[i]);
                if (buttonId >= 0)
                {
                    AuxButtons[buttonId] = true;
                }
            }
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }

        private static int MapToButton(byte report1, byte buttonId)
        {
            return buttonId switch
            {
                0x3E => 0,
                0x0C => 1,
                0x2C => 2,
                // Buttons 4 and 6 could shadow each other in the case of more than one buttons being pressed at once.
                // Button 6 would take precedence.
                0x19 => report1 == 0x00 ? 3 : 5,
                0x06 => 4,
                0x1D => 6,
                0x16 => 7,
                0x2E => 8,
                0x2D => 9,
                0x30 => 10,
                0x2F => 11,
                _ => -1
            };
        }
    }
}
