using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletReference
    {
        public TabletReference()
        {
        }

        public TabletReference(
            TabletConfiguration properties,
            IEnumerable<DeviceIdentifier> identifiers
        )
        {
            this.Properties = properties;
            this.Identifiers = identifiers;
        }

        public TabletConfiguration Properties { set; get; }
        public IEnumerable<DeviceIdentifier> Identifiers { set; get; }
    }
}
