using HidSharp;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class TabletMatch<T> where T : DeviceIdentifier
    {
        public TabletMatch(TabletConfiguration configuration, HidDevice device, T[] identifiers)
        {
            Configuration = configuration;
            Device = device;
            Identifiers = identifiers;
        }

        public TabletConfiguration Configuration { get; init; }
        public HidDevice Device { get; init; }
        public T[] Identifiers { get; init; }
    }
}