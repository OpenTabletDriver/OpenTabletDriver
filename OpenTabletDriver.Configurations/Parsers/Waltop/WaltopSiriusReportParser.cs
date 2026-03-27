using System;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    /// <summary>
    /// Report parser for Waltop Sirius Battery Free Tablet in tablet mode (4000 LPI).
    ///
    /// Problem:
    /// The pen uses EMR (electromagnetic resonance) where barrel buttons work by switching
    /// capacitors into the pen's resonant circuit. Button states are physically mutually
    /// exclusive — only one button can be active at a time. In default mode (Report ID 0x10),
    /// the firmware correctly encodes this as a 2-bit enumeration (values 2=barrel, 3=pick).
    ///
    /// However, in tablet mode (Report ID 0x02, required for 4000 LPI), the firmware encodes
    /// buttons as individual bits (data[5] bit 3 and bit 4). Because the EMR signal is
    /// inherently ambiguous between the two button states, the firmware produces a noisy
    /// alternating pattern on both bits — making it appear as if neither button can be
    /// reliably identified from a single report.
    ///
    /// Solution:
    /// A Sequential Probability Ratio Test (SPRT) classifier exploits the statistical
    /// difference between the two buttons. Empirical measurement in default mode (where
    /// buttons are cleanly identified) showed:
    ///   - Barrel button (upper physical): produces bit 3 ~99% of the time, bit 4 ~1%
    ///   - Pick button (lower physical):   produces bit 3 ~8% of the time, bit 4 ~92%
    ///
    /// When a button press begins (any button bit set after idle), the classifier accumulates
    /// a log-likelihood ratio from each report. Once the ratio exceeds a confidence threshold,
    /// the button identity is locked until release (all button bits clear). This typically
    /// converges within 2-5 reports (~10-25ms at 200 RPS), well below perceptible latency.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WaltopSiriusReportParser : IReportParser<IDeviceReport>
    {
        private enum ButtonState { Idle, Classifying, LockedBarrel, LockedPick }

        // Log-likelihood ratio increments per observation, derived from empirical
        // button signal distributions measured in default mode:
        //   bit3 observation: log(P(bit3|barrel) / P(bit3|pick)) = log(0.99/0.08) = +2.516
        //   bit4 observation: log(P(bit4|barrel) / P(bit4|pick)) = log(0.01/0.92) = -4.522
        // Positive LLR = evidence for barrel, negative = evidence for pick.
        private const double LlrBit3 = 2.516;
        private const double LlrBit4 = -4.522;
        private const double Threshold = 4.6; // ln(99) ≈ 4.6, corresponds to ~99% confidence

        private ButtonState _state = ButtonState.Idle;
        private double _llr;

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
                                _state = ButtonState.Classifying;
                                _llr = 0;
                                goto case ButtonState.Classifying;
                            }
                            break;

                        case ButtonState.Classifying:
                            if (!anyButton)
                            {
                                _state = ButtonState.Idle;
                                break;
                            }
                            _llr += bit4 ? LlrBit4 : LlrBit3;
                            if (_llr >= Threshold)
                            {
                                _state = ButtonState.LockedBarrel;
                                reportBarrel = true;
                            }
                            else if (_llr <= -Threshold)
                            {
                                _state = ButtonState.LockedPick;
                                reportPick = true;
                            }
                            else
                            {
                                // Not yet decided — provisionally report the more likely button
                                reportBarrel = _llr > 0;
                                reportPick = _llr <= 0;
                            }
                            break;

                        case ButtonState.LockedBarrel:
                            if (!anyButton)
                                _state = ButtonState.Idle;
                            else
                                reportBarrel = true;
                            break;

                        case ButtonState.LockedPick:
                            if (!anyButton)
                                _state = ButtonState.Idle;
                            else
                                reportPick = true;
                            break;
                    }

                    byte classifiedFlags = (byte)(
                        (data[5] & 0x07) |           // preserve proximity (bits 0-1) + tip (bit 2)
                        (reportBarrel ? 0x08 : 0) |  // bit 3 = barrel
                        (reportPick ? 0x10 : 0)      // bit 4 = pick
                    );

                    return new WaltopSiriusTabletReport(data, classifiedFlags);
                }

                // Frame button report (tablet mode only)
                case 0x0A when data[1] == 0x0E:
                    return new WaltopSiriusAuxReport(data);

                default:
                    return new DeviceReport(data);
            }
        }
    }
}
