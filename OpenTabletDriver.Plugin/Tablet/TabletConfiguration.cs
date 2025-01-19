﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletConfiguration
    {
        /// <summary>
        /// The tablet's name.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(Name)} is required")]
        public string? Name { set; get; } = string.Empty;

        /// <summary>
        /// The tablet's specifications.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(Specifications)} is required")]
        public TabletSpecifications? Specifications { set; get; } = new TabletSpecifications();

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        [Required(ErrorMessage = $"Tablet {nameof(DigitizerIdentifiers)} are required")]
        [MinLength(1, ErrorMessage = "Requires at least 1 identifier")]
        public List<DeviceIdentifier>? DigitizerIdentifiers { set; get; } = [];

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        public List<DeviceIdentifier>? AuxilaryDeviceIdentifiers { set; get; }

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string>? Attributes { set; get; }
    }
}
