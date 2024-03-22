using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Veikk
{
    public struct VeikkAuxV1Report : IAuxReport
    {
        public VeikkAuxV1Report(byte[] report)
        {
            Raw = report;
            AuxButtons = new bool[]
            {
                // Buttons 4 and 6 could shadow each other in the case of more than one buttons being pressed at once.
                // Button 6 would take precedence.
                System.Array.IndexOf(report, (byte)0x3E) > 1,
                System.Array.IndexOf(report, (byte)0x0C) > 1,
                System.Array.IndexOf(report, (byte)0x2C) > 1,
                System.Array.IndexOf(report, (byte)0x19) > 1 && report[1] == 0x00,
                System.Array.IndexOf(report, (byte)0x06) > 1,
                System.Array.IndexOf(report, (byte)0x19) > 1 && report[1] == 0x01,
                System.Array.IndexOf(report, (byte)0x1D) > 1,
                System.Array.IndexOf(report, (byte)0x16) > 1,
                System.Array.IndexOf(report, (byte)0x2E) > 1,
                System.Array.IndexOf(report, (byte)0x2D) > 1,
                System.Array.IndexOf(report, (byte)0x30) > 1,
                System.Array.IndexOf(report, (byte)0x2F) > 1,
            };
        }

        public byte[] Raw { get; set; }
        public bool[] AuxButtons { get; set; }
    }
}
