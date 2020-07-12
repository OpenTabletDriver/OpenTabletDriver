namespace TabletDriverPlugin.Tablet
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
        public uint InputReportLength { set; get; }

        /// <summary>
        /// The maximum output report length reported by the device.
        /// </summary>
        public uint OutputReportLength { set; get; }

        /// <summary>
        /// The device report parser used by the detected device.
        /// </summary>
        public string ReportParser { set; get; }

        /// <summary>
        /// The feature report sent to initialize tablet functions.
        /// </summary>
        public byte[] FeatureInitReport { set; get; }

        /// <summary>
        /// The output report sent to initialize tablet functions.
        /// </summary>
        public byte[] OutputInitReport { set; get; }
    }
}