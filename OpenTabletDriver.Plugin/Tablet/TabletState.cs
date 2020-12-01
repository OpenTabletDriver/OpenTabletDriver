namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletState
    {
        public TabletState(
            TabletConfiguration tabletProperties,
            DigitizerIdentifier digitizer,
            DeviceIdentifier auxiliary
        )
        {
            TabletProperties = tabletProperties;
            Digitizer = digitizer;
            Auxiliary = auxiliary;
        }

        public TabletConfiguration TabletProperties { get; }
        public DigitizerIdentifier Digitizer { get; }
        public DeviceIdentifier Auxiliary { get; }
    }
}
