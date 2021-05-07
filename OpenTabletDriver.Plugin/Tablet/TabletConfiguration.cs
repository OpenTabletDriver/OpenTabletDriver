using System.Collections.Generic;
using System.Linq;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletConfiguration
    {
        /// <summary>
        /// The tablet's name.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// The tablet's specifications.
        /// </summary>
        public TabletSpecifications Specifications { set; get; } = new TabletSpecifications();

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        public List<DeviceIdentifier> DigitizerIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        public List<DeviceIdentifier> AuxilaryDeviceIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string> Attributes { set; get; } = new Dictionary<string, string>();

        /// <summary>
        /// Retrieves the VendorID specified by the first instance of <see cref="DigitizerIdentifiers"/>
        /// </summary>
        public int CompatibleVendorID => DigitizerIdentifiers.First().VendorID;
    }
}
