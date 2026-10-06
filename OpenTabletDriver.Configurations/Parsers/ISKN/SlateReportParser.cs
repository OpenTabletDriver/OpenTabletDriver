using System.Diagnostics;
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
        private const long ButtonHoldMs = 100;

        private readonly Stopwatch _buttonHeld = new();
        private byte _buttonCode;

        public IDeviceReport Parse(byte[] report)
        {
            if (_buttonHeld.IsRunning)
            {
                // The tablet only reports button presses, so hold the button for a short
                // while and release it on the first report after that.
                if (_buttonHeld.ElapsedMilliseconds < ButtonHoldMs)
                    return new SlateAuxReport(report, _buttonCode);
                _buttonHeld.Reset();
                return new SlateAuxReport(report, 0);
            }

            if (report.Length < 6 || report[1] != 0xB3 || report[2] != 0xA5 || report[3] != 0xE1)
                return new DeviceReport(report);

            switch (report[4])
            {
                case PenFrame when report.Length >= 18:
                    return new SlateTabletReport(report);
                case ButtonFrame:
                    _buttonCode = report[5];
                    _buttonHeld.Restart();
                    return new SlateAuxReport(report, _buttonCode);
                default:
                    return new DeviceReport(report);
            }
        }
    }
}
