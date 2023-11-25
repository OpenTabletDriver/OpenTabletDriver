using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device identifier.
    /// </summary>
    [PublicAPI]
    public class DeviceIdentifier
    {
        /// <summary>
        /// The Vendor ID of the device.
        /// </summary>
        [DisplayName("Vendor ID")]
        [Required(ErrorMessage = $"{nameof(VendorID)} identifier must be defined")]
        [Range(0, 0xFFFF)]
        public int VendorID { set; get; }

        /// <summary>
        /// The Product ID of the device.
        /// </summary>
        [DisplayName("Product ID")]
        [Required(ErrorMessage = $"{nameof(ProductID)} identifier must be defined")]
        [Range(0, 0xFFFF)]
        public int ProductID { set; get; }

        /// <summary>
        /// The maximum input report length reported by the device.
        /// </summary>
        [DisplayName("Input Report Length")]
        public uint? InputReportLength { set; get; }

        /// <summary>
        /// The maximum output report length reported by the device.
        /// </summary>
        [DisplayName("Output Report Length")]
        public uint? OutputReportLength { set; get; }

        /// <summary>
        /// The device report parser used by the detected device.
        /// </summary>
        [DisplayName("Report Parser")]
        [RegularExpression(@"^([A-Za-z]+\w*)(\.[A-Za-z]+\w*)+$", ErrorMessage = $"{nameof(ReportParser)} for identifier must match regular expression")]
        public string ReportParser { set; get; } = string.Empty;

        /// <summary>
        /// The feature report sent to initialize tablet functions.
        /// </summary>
        [DisplayName("Feature Initialization Report")]
        public List<byte[]>? FeatureInitReport { set; get; }

        /// <summary>
        /// The output report sent to initialize tablet functions.
        /// </summary>
        [DisplayName("Output Initialization Report")]
        public List<byte[]>? OutputInitReport { set; get; }

        /// <summary>
        /// Device strings to match against, used for identification.
        /// </summary>
        [DisplayName("Device Strings")]
        public Dictionary<byte, string> DeviceStrings { set; get; } = new Dictionary<byte, string>();

        /// <summary>
        /// Device strings to query to initialize device endpoints.
        /// </summary>
        [DisplayName("Initialization Strings")]
        public List<byte> InitializationStrings { set; get; } = new List<byte>();
    }
}
