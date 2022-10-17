using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenTabletDriver.Tablet
{
    public class DeviceMetadata
    {
        /// <summary>
        /// Aliases for the device's model name.
        /// </summary>
        public List<string> Aliases { set; get; } = new List<string>();

        /// <summary>
        /// The device's current support status.
        /// </summary>
        [DisplayName("Support Status")]
        public DeviceSupportStatus SupportStatus { set; get; } = DeviceSupportStatus.Untested;

        /// <summary>
        /// Features supported by the report parsers.
        /// </summary>
        public List<string> Features { set; get; } = new List<string>();

        /// <summary>
        /// Features not yet supported by the report parsers.
        /// </summary>
        [DisplayName("Unsupported Features")]
        public List<string> UnsupportedFeatures { set; get; } = new List<string>();

        /// <summary>
        /// Additional information about the device.
        /// </summary>
        public List<string> Notes { set; get; } = new List<string>();
    }
}
