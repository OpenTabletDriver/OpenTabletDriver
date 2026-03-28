using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Report parser for Waltop Sirius Battery Free Tablet in tablet mode (4000 LPI).
    ///
    /// Tablet mode is activated by the following FeatureInitReport sequence
    /// (HID Set Feature, Report ID 0x02, 3 bytes each):
    ///
    ///   02 10 01  — switch to tablet mode (4000 LPI, Report ID 0x02 pen reports)
    ///   02 11 0F  — enable macro/frame key reporting (Report ID 0x0A)
    ///   02 18 04  — set resolution mode
    ///   02 12 FF  — enable EMR autogain
    ///   02 17 00  — enable firmware signal filtering
    ///
    /// These match the initialization sequence used by the yamato6537/waltop-linux
    /// kernel driver. The autogain and filter commands may improve EMR signal quality.
    ///
    /// In tablet mode, pen side buttons are encoded as individual bits in data[5]:
    /// bit 3 and bit 4. The signal is noisy — the dominant bit identifies the button
    /// (~92% of frames), but sporadic flips to the other bit occur (~8%).
    ///
    /// A simple majority-vote over the first few decisive frames locks the button
    /// identity until release. This converges within 2-3 frames (~10-15ms at 200 RPS).
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WaltopSiriusReportParser : IReportParser<IDeviceReport>
    {
        private enum ButtonState { Idle, Classifying, LockedBarrel, LockedPick }

        private const int VotesNeeded = 3;
        private const int ReleaseFrames = 2;

        private ButtonState _state = ButtonState.Idle;
        private int _bit3Count;
        private int _bit4Count;
        private int _clearCount;

        public IDeviceReport Parse(byte[] data)
        {
            switch (data[0])
            {
                case 0x02:
                {
                    bool inRange = (data[5] & 0x03) != 0;

                    if (!inRange)
                    {
                        _state = ButtonState.Idle;
                        return new OutOfRangeReport(data);
                    }

                    bool bit3 = (data[5] & 0x08) != 0;
                    bool bit4 = (data[5] & 0x10) != 0;
                    bool anyButton = bit3 || bit4;

                    bool reportBarrel = false;
                    bool reportPick = false;

                    switch (_state)
                    {
                        case ButtonState.Idle:
                            if (anyButton)
                            {
                                _bit3Count = 0;
                                _bit4Count = 0;
                                _clearCount = 0;
                                _state = ButtonState.Classifying;
                                goto case ButtonState.Classifying;
                            }
                            break;

                        case ButtonState.Classifying:
                            if (!anyButton)
                            {
                                if (++_clearCount >= ReleaseFrames)
                                    _state = ButtonState.Idle;
                                break;
                            }

                            _clearCount = 0;

                            if (bit3 && !bit4) _bit3Count++;
                            if (bit4 && !bit3) _bit4Count++;

                            if (_bit3Count >= VotesNeeded)
                            {
                                _state = ButtonState.LockedBarrel;
                                reportBarrel = true;
                            }
                            else if (_bit4Count >= VotesNeeded)
                            {
                                _state = ButtonState.LockedPick;
                                reportPick = true;
                            }
                            break;

                        case ButtonState.LockedBarrel:
                            if (!anyButton)
                            {
                                if (++_clearCount >= ReleaseFrames)
                                    _state = ButtonState.Idle;
                                else
                                    reportBarrel = true;
                            }
                            else
                            {
                                _clearCount = 0;
                                reportBarrel = true;
                            }
                            break;

                        case ButtonState.LockedPick:
                            if (!anyButton)
                            {
                                if (++_clearCount >= ReleaseFrames)
                                    _state = ButtonState.Idle;
                                else
                                    reportPick = true;
                            }
                            else
                            {
                                _clearCount = 0;
                                reportPick = true;
                            }
                            break;
                    }

                    byte classifiedFlags = (byte)(
                        (data[5] & 0x07) |           // preserve proximity (bits 0-1) + tip (bit 2)
                        (reportBarrel ? 0x08 : 0) |  // bit 3 = barrel (upper button)
                        (reportPick ? 0x10 : 0)      // bit 4 = pick (lower button)
                    );

                    return new WaltopSiriusTabletReport(data, classifiedFlags);
                }

                // Frame button report (tablet mode only)
                case 0x0A when data[1] == 0x0E:
                    return new WaltopSiriusAuxReport(data);

                // Dial reports: scroll (0x02), zoom (0x03), volume (0x04)
                // Same data layout, firmware switches subreport based on mode
                case 0x0A when data[1] >= 0x02 && data[1] <= 0x04:
                    return new WaltopSiriusDialReport(data);

                // Keyboard-direction dial report (arrow keys / navigation keys)
                case 0x0D:
                    return new WaltopSiriusKeyDialReport(data);

                // Border location report (K1-K10 virtual buttons on top edge)
                case 0x05:
                    return new WaltopSiriusBorderReport(data);

                default:
                    return new DeviceReport(data);
            }
        }
    }
}
