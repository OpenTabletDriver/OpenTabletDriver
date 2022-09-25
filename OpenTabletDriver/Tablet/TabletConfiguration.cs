using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A tablet device configuration. Provides everything needed to detect and configure a device.
    /// </summary>
    [PublicAPI]
    public class TabletConfiguration
    {
        /// <summary>
        /// The manufacturer of the device.
        /// </summary>
        public string Manufacturer { set; get; } = string.Empty;

        /// <summary>
        /// The model name of the device.
        /// </summary>
        public string Model { set; get; } = string.Empty;

        /// <summary>
        /// The tablet's specifications.
        /// </summary>
        public TabletSpecifications Specifications { set; get; } = new TabletSpecifications();

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        [DisplayName("Digitizer Identifiers")]
        public List<DeviceIdentifier> DigitizerIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        [DisplayName("Auxiliary Identifiers")]
        public List<DeviceIdentifier> AuxiliaryDeviceIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// The tablet's metadata. This is used for generating the supported tablets list.
        /// </summary>
        public DeviceMetadata Metadata { set; get; } = new DeviceMetadata();

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string> Attributes { set; get; } = new Dictionary<string, string>();

        public override string ToString() => $"{Manufacturer} {Model}";
    }
}
