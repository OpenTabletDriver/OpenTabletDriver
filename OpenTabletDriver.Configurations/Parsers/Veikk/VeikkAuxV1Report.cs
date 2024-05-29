using System;
using System.Collections.Generic;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkAuxV1Report : IAuxReport
    {
        private int AuxButtonDict(byte report1, byte buttonId)
        {
            switch (buttonId)
            {
                default: return -1;
                case 0x3E: return 0;
                case 0x0C: return 1;
                case 0x2C: return 2;
                // Buttons 4 and 6 could shadow each other in the case of more than one buttons being pressed at once.
                // Button 6 would take precedence.
                case 0x19:
                {
                    if (report1 == 0x00) return 3;
                    return 5;
                }
                case 0x06: return 4;
                case 0x1D: return 6;
                case 0x16: return 7;
                case 0x2E: return 8;
                case 0x2D: return 9;
                case 0x30: return 10;
                case 0x2F: return 11;
            }

        }

        public VeikkAuxV1Report(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[12];
            for (var i = 2; i < report.Length; i++)
            {
                var buttonId = AuxButtonDict(report[1], report[i]);
                if (buttonId >= 0)
                {
                    AuxButtons[buttonId] = true;
                }

            }
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
