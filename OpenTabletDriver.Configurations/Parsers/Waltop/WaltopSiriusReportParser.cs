using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Waltop
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WaltopSiriusReportParser : IReportParser<IDeviceReport>
    {
        private const int ButtonBitsMask = 0x18; // bits 3-4 (barrel button — both map to same)
        private const long DebounceMs = 30;

        private int _stableButtons;
        private int _pendingButtons;
        private long _pendingTimestamp;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public IDeviceReport Parse(byte[] data)
        {
            switch (data[0])
            {
                // Pen report (tablet mode, 4000 LPI)
                case 0x02:
                {
                    bool inRange = (data[5] & 0x03) != 0;

                    if (!inRange)
                        return new OutOfRangeReport(data);

                    int rawButtons = data[5] & ButtonBitsMask;
                    int debouncedButtons = Debounce(rawButtons);

                    // Reconstruct flags: keep proximity/tip from raw, use debounced buttons
                    byte debouncedFlags = (byte)((data[5] & ~ButtonBitsMask) | debouncedButtons);

                    return new WaltopSiriusTabletReport(data, debouncedFlags);
                }

                // Frame button report
                case 0x0A when data[1] == 0x0E:
                    return new WaltopSiriusAuxReport(data);

                default:
                    return new DeviceReport(data);
            }
        }

        private int Debounce(int rawButtons)
        {
            if (rawButtons == _stableButtons)
            {
                // No change from stable state; reset pending
                _pendingButtons = rawButtons;
                return _stableButtons;
            }

            long now = _stopwatch.ElapsedMilliseconds;

            if (rawButtons != _pendingButtons)
            {
                // New transition — start tracking
                _pendingButtons = rawButtons;
                _pendingTimestamp = now;
                return _stableButtons;
            }

            // Same pending state — check if stable long enough
            if (now - _pendingTimestamp >= DebounceMs)
            {
                _stableButtons = rawButtons;
                return _stableButtons;
            }

            return _stableButtons;
        }
    }
}
