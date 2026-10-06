using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ISKN
{
    /// <summary>
    /// Parses the vendor HID channel of the iskn Slate (report id 3).
    /// Every report carries one protocol frame: b3 a5 e1 | type | payload | crc16-xmodem(payload) LE.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class SlateReportParser : IReportParser<IDeviceReport>
    {
        private const byte PenFrame = 0x05;
        private const byte ButtonFrame = 0x08;

        private bool _buttonPressed;

        public IDeviceReport Parse(byte[] report)
        {
            if (_buttonPressed)
            {
                // The tablet sends a single frame per button gesture, on release, and never
                // reports the button going back up. Release it on the following report instead:
                // the next pen sample while the pen is in range, or at worst the heartbeat
                // frame the tablet sends every second.
                _buttonPressed = false;
                return new SlateAuxReport(report, released: true);
            }

            if (report.Length < 6 || report[1] != 0xB3 || report[2] != 0xA5 || report[3] != 0xE1)
                return new DeviceReport(report);

            switch (report[4])
            {
                case PenFrame when report.Length >= 18:
                    return new SlateTabletReport(report);
                case ButtonFrame when report.Length >= 6:
                    _buttonPressed = true;
                    return new SlateAuxReport(report);
                default:
                    return new DeviceReport(report);
            }
        }
    }
}
