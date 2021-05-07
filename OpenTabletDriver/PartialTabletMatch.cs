using HidSharp;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class PartialTabletMatch
    {
        public HidDevice Device { get; init; }
        public TabletConfiguration TabletConfiguration { get; init; }
        public DeviceIdentifier[] DigitizerIdentifiers { get; init; }
        public DeviceIdentifier[] AuxilaryIdentifiers { get; init; }
    }
}