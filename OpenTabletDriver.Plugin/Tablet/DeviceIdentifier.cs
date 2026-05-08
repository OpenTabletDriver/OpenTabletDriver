using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#nullable enable

namespace OpenTabletDriver.Plugin.Tablet
{
    public class DeviceIdentifier
    {
        /// <summary>
        /// The Vendor ID of the device.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(VendorID)} identifier must be defined")]
        [Range(0, 0xFFFF)]
        [JsonProperty(nameof(VendorID))]
        public int VendorID { set; get; }

        /// <summary>
        /// The Product ID of the device.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(ProductID)} identifier must be defined")]
        [Range(0, 0xFFFF)]
        [JsonProperty(nameof(ProductID))]
        public int ProductID { set; get; }

        /// <summary>
        /// The maximum input report length reported by the device.
        /// </summary>
        [JsonProperty(nameof(InputReportLength))]
        public uint? InputReportLength { set; get; }

        /// <summary>
        /// The maximum output report length reported by the device.
        /// </summary>
        [JsonProperty(nameof(OutputReportLength))]
        public uint? OutputReportLength { set; get; }

        /// <summary>
        /// The maximum feature report length reported by the device.
        /// </summary>
        [JsonProperty(nameof(FeatureReportLength))]
        public uint? FeatureReportLength { set; get; }

        /// <summary>
        /// The device report parser used by the detected device.
        /// </summary>
        [RegularExpression(@"^([A-Za-z]+\w*)(\.[A-Za-z]+\w*)+$", ErrorMessage = $"{nameof(ReportParser)} for identifier must match regular expression")]
        [JsonProperty(nameof(ReportParser))]
        public string ReportParser { set; get; } = typeof(PassthroughReportParser).FullName!;

        /// <summary>
        /// The feature report sent to initialize tablet functions.
        /// </summary>
        [JsonProperty(nameof(FeatureInitReport))]
        public List<byte[]>? FeatureInitReport { set; get; }

        /// <summary>
        /// The output report sent to initialize tablet functions.
        /// </summary>
        [JsonProperty(nameof(OutputInitReport))]
        public List<byte[]>? OutputInitReport { set; get; }

        /// <summary>
        /// Device strings to match against, used for identification.
        /// </summary>
        /// <typeparam name="byte">The index to query</typeparam>
        /// <typeparam name="string">The value to match to the queried index</typeparam>
        [JsonProperty(nameof(DeviceStrings))]
        public Dictionary<byte, string>? DeviceStrings { set; get; }

        /// <summary>
        /// Device strings to query to initialize device endpoints.
        /// </summary>
        [JsonProperty(nameof(InitializationStrings))]
        public List<byte>? InitializationStrings { set; get; }

        /// <summary>
        /// Arbitrary devices attributes
        /// </summary>
        [JsonProperty(nameof(Attributes))]
        public Dictionary<string, string>? Attributes { set; get; }
    }
}
