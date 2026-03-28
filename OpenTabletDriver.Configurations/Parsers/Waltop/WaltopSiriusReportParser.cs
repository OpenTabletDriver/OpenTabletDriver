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
    /// buttons as individual bits (data[5] bit 3 and bit 4). In practice the pen can still
    /// emit mixed frames at press onset (including both bits set), so button identity cannot
    /// be trusted from a single report.
    ///
    /// Solution:
    /// A Sequential Probability Ratio Test (SPRT) classifier exploits the statistical
    /// difference between the two buttons. Empirical measurement in default mode (where
    /// buttons are cleanly identified) showed:
    ///   - Barrel button (upper physical): produces bit 3 ~99% of the time, bit 4 ~1%
    ///   - Pick button (lower physical):   produces bit 3 ~8% of the time, bit 4 ~92%
    ///
    /// When a button press begins (any side-button bit set after idle), the classifier accumulates
    /// a log-likelihood ratio from decisive reports only. Mixed frames (both bits set) are treated
    /// as ambiguous and do not bias the decision. Until enough decisive observations are gathered
    /// and the ratio exceeds the confidence threshold, side-button output is intentionally
    /// suppressed to avoid false binding edges. Once the threshold is crossed, the button identity
    /// is locked until a short release hysteresis expires or the pen leaves range.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WaltopSiriusReportParser : IReportParser<IDeviceReport>
    {
        private enum ButtonState { Idle, Classifying, LockedBarrel, LockedPick }
        private enum Observation { None, Bit3Only, Bit4Only, Ambiguous }

        // Log-likelihood ratio increments per observation, derived from empirical
        // button signal distributions measured in default mode:
        //   bit3 observation: log(P(bit3|barrel) / P(bit3|pick)) = log(0.99/0.08) = +2.516
        //   bit4 observation: log(P(bit4|barrel) / P(bit4|pick)) = log(0.01/0.92) = -4.522
        // Positive LLR = evidence for barrel, negative = evidence for pick.
        private const double LlrBit3 = 2.516;
        private const double LlrBit4 = -4.522;
        private const double Threshold = 4.6; // ln(99) ≈ 4.6, corresponds to ~99% confidence
        private const int MinimumDecisiveObservations = 3;
        private const int ReleaseHysteresisFrames = 2;

        private ButtonState _state = ButtonState.Idle;
        private double _llr;
        private int _decisiveObservations;
        private int _clearFrames;

        public IDeviceReport Parse(byte[] data)
        {
            switch (data[0])
            {
                case 0x02:
                {
                    bool inRange = (data[5] & 0x03) != 0;

                    if (!inRange)
                    {
                        ResetClassifier();
                        return new OutOfRangeReport(data);
                    }

                    var observation = GetObservation(data[5]);
                    bool anyButton = observation != Observation.None;

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
                            if (observation == Observation.None)
                            {
                                if (++_clearFrames >= ReleaseHysteresisFrames)
                                    ResetClassifier();
                                break;
                            }

                            _clearFrames = 0;

                            if (observation == Observation.Ambiguous)
                                break;

                            _decisiveObservations++;
                            _llr += observation == Observation.Bit4Only ? LlrBit4 : LlrBit3;

                            if (_decisiveObservations >= MinimumDecisiveObservations && _llr >= Threshold)
                            {
                                _state = ButtonState.LockedBarrel;
                                reportBarrel = true;
                            }
                            else if (_decisiveObservations >= MinimumDecisiveObservations && _llr <= -Threshold)
                            {
                                _state = ButtonState.LockedPick;
                                reportPick = true;
                            }
                            break;

                        case ButtonState.LockedBarrel:
                            if (observation == Observation.None)
                            {
                                if (++_clearFrames >= ReleaseHysteresisFrames)
                                    ResetClassifier();
                                else
                                    reportBarrel = true;
                            }
                            else
                            {
                                _clearFrames = 0;
                                reportBarrel = true;
                            }
                            break;

                        case ButtonState.LockedPick:
                            if (observation == Observation.None)
                            {
                                if (++_clearFrames >= ReleaseHysteresisFrames)
                                    ResetClassifier();
                                else
                                    reportPick = true;
                            }
                            else
                            {
                                _clearFrames = 0;
                                reportPick = true;
                            }
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

        private void ResetClassifier()
        {
            _state = ButtonState.Idle;
            _llr = 0;
            _decisiveObservations = 0;
            _clearFrames = 0;
        }

        private static Observation GetObservation(byte flags)
        {
            return (flags & 0x18) switch
            {
                0x08 => Observation.Bit3Only,
                0x10 => Observation.Bit4Only,
                0x18 => Observation.Ambiguous,
                _ => Observation.None
            };
        }
    }
}
