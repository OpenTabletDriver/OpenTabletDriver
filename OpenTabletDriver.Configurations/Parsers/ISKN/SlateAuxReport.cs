using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ISKN
{
    /// <summary>
    /// Tablet button event (frame type 0x08), sent once when a button is released. The payload
    /// byte tells which button and whether it was a short tap or a long press:
    /// 0x03 top tap, 0x02 bottom tap, 0x05 top long press, 0x04 bottom long press,
    /// 0x09 both held. The latter also makes the tablet recalibrate its magnetic sensors,
    /// so it is better left unbound.
    /// </summary>
    public struct SlateAuxReport : IAuxReport
    {
        private const int CodeIndex = 5;

        public SlateAuxReport(byte[] report, bool released = false)
        {
            Raw = report;
            var code = released ? 0 : report[CodeIndex];
            AuxButtons =
            [
                code == 0x03,
                code == 0x02,
                code == 0x05,
                code == 0x04,
                code == 0x09,
            ];
        }

        public byte[] Raw { set; get; }
        public bool[] AuxButtons { set; get; }
    }
}
