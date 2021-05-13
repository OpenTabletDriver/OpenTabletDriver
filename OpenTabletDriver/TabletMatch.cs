using HidSharp;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class TabletMatch
    {
        public TabletConfiguration Configuration { get; init; }
        public HidDevice Device { get; init; }
        public DeviceIdentifier Identifier { get; init; }
        public bool IsDigitizer { get; init; }
    }
}