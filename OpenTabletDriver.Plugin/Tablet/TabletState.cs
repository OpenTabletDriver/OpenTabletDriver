namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletState
    {
        public TabletState()
        {
        }

        public TabletState(
            TabletConfiguration properties,
            DeviceIdentifier digitizer,
            DeviceIdentifier auxiliary
        )
        {
            Properties = properties;
            Digitizer = digitizer;
            Auxiliary = auxiliary;
        }

        public TabletConfiguration Properties { set; get; }
        public DeviceIdentifier Digitizer { set; get; }
        public DeviceIdentifier Auxiliary { set; get; }
    }
}
