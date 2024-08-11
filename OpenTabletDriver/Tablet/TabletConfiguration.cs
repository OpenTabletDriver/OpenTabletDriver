using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// The tablet's name.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(Name)} is required")]
        public string Name { set; get; } = string.Empty;

        /// <summary>
        /// The tablet's specifications.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(Specifications)} is required")]
        public TabletSpecifications Specifications { set; get; } = new TabletSpecifications();

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(DigitizerIdentifiers)} are required")]
        [DisplayName("Digitizer Identifiers")]
        [MinLength(1, ErrorMessage = "Requires at least 1 identifier")]
        public List<DeviceIdentifier> DigitizerIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(AuxiliaryDeviceIdentifiers)} must be present")]
        [DisplayName("Auxiliary Identifiers")]
        public List<DeviceIdentifier> AuxiliaryDeviceIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(Attributes)} must be present")]
        public Dictionary<string, string> Attributes { set; get; } = new Dictionary<string, string>();
    }
}
