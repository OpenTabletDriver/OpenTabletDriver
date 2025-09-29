using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#nullable enable

namespace OpenTabletDriver.Plugin.Tablet
{
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
        [MinLength(1, ErrorMessage = "Requires at least 1 identifier")]
        public List<DeviceIdentifier> DigitizerIdentifiers { set; get; } = new List<DeviceIdentifier>();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        public List<DeviceIdentifier>? AuxiliaryDeviceIdentifiers { set; get; }

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string>? Attributes { set; get; }

        #region Legacy Properties

        // ReSharper disable twice IdentifierTypo
        private List<DeviceIdentifier>? _auxilaryDeviceIdentifiers;

        [Obsolete(Globals.LegacyTabletConfigurationProperty)]
        [JsonIgnore]
        public List<DeviceIdentifier>? AuxilaryDeviceIdentifiers
        {
            set => _auxilaryDeviceIdentifiers = AuxiliaryDeviceIdentifiers = value;
            get => AuxiliaryDeviceIdentifiers;
        }

        // hack which allows us to deserialize the object for backwards compatibility, but not emit it in serialization
        [JsonProperty("AuxilaryDeviceIdentifiers")]
#pragma warning disable CS0618 // Type or member is obsolete
        private List<DeviceIdentifier>? AuxilaryDeviceIdentifiers2
        {
            set => AuxilaryDeviceIdentifiers = value;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public bool HasLegacyProperties()
        {
            return _auxilaryDeviceIdentifiers is { Count: > 0 };
        }

        #endregion
    }
}
