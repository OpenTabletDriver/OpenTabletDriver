using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver
{
    public class TabletStatus
    {
        public TabletStatus(
            TabletConfiguration tabletProperties,
            DigitizerIdentifier tabletIdentifier,
            DeviceIdentifier auxiliaryIdentifier
        )
        {
            TabletProperties = tabletProperties;
            TabletIdentifier = tabletIdentifier;
            AuxiliaryIdentifier = auxiliaryIdentifier;
        }

        public TabletConfiguration TabletProperties { protected set; get; }
        public DigitizerIdentifier TabletIdentifier { private set; get; }
        public DeviceIdentifier AuxiliaryIdentifier { private set; get; }
    }
}