using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class DeviceIdentifier
    {
        /// <summary>
        /// The Vendor ID of the device.
        /// </summary>
        public int VendorID { set; get; }

        /// <summary>
        /// The Product ID of the device.
        /// </summary>
        public int ProductID { set; get; }

        /// <summary>
        /// The maximum input report length reported by the device.
        /// </summary>
        public uint? InputReportLength { set; get; }

        /// <summary>
        /// The maximum output report length reported by the device.
        /// </summary>
        public uint? OutputReportLength { set; get; }

        /// <summary>
        /// The device report parser used by the detected device.
        /// </summary>
        public string ReportParser { set; get; }

        /// <summary>
        /// The feature report sent to initialize tablet functions.
        /// </summary>
        public List<byte[]> FeatureInitReport { set; get; }

        /// <summary>
        /// The output report sent to initialize tablet functions.
        /// </summary>
        public List<byte[]> OutputInitReport { set; get; }

        /// <summary>
        /// Device strings to match against, used for identification.
        /// </summary>
        /// <typeparam name="uint">The index to query</typeparam>
        /// <typeparam name="string">The value to match to the queried index</typeparam>
        public Dictionary<byte, string> DeviceStrings { set; get; } = new Dictionary<byte, string>();

        /// <summary>
        /// Device strings to query to initialize device endpoints.
        /// </summary>
        public List<byte> InitializationStrings { set; get; } = new List<byte>();

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string> Attributes { set; get; } = new Dictionary<string, string>();
    }
}