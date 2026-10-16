using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Devices
{
    public class SerializedDeviceEndpoint
    {
        public SerializedDeviceEndpoint()
        {
            DeviceAttributes = new Dictionary<string, string>();
        }

        [SetsRequiredMembers]
        public SerializedDeviceEndpoint(IDeviceEndpoint endpoint)
        {
            DevicePath = endpoint.DevicePath;
            Manufacturer = endpoint.Manufacturer;
            ProductName = endpoint.ProductName;
            FriendlyName = endpoint.FriendlyName;
            ProductID = endpoint.ProductID;
            VendorID = endpoint.VendorID;
            InputReportLength = endpoint.InputReportLength;
            OutputReportLength = endpoint.OutputReportLength;
            FeatureReportLength = endpoint.FeatureReportLength;
            SerialNumber = endpoint.SerialNumber;
            CanOpen = endpoint.CanOpen;
            DeviceAttributes = endpoint.DeviceAttributes;
        }

        public int VendorID { get; set; }

        public int ProductID { get; set; }

        public int InputReportLength { get; set; }

        public int OutputReportLength { get; set; }

        public int FeatureReportLength { get; set; }

        public bool CanOpen { get; set; }

        public IDictionary<string, string> DeviceAttributes { get; set; }

        [JsonProperty(nameof(Manufacturer))]
        public string? Manufacturer { get; set; }

        [JsonProperty(nameof(ProductName))]
        public string? ProductName { get; set; }

        [JsonProperty(nameof(SerialNumber))]
        public string? SerialNumber { get; set; }

        [JsonProperty(nameof(FriendlyName))]
        public string? FriendlyName { get; set; }

        [JsonProperty(nameof(DevicePath))]
        public required string DevicePath { get; set; }

        /// <returns>A 2-line string consisting of important device distinctions</returns>
        public override string ToString()
        {
            // don't include manufacturer if it's already in the product name (e.g. Razer)
            string name = ProductName ?? FriendlyName ?? "null";
            string manufacturer = Manufacturer ?? "null";
            string title = name.Contains(manufacturer) ? name : $"{manufacturer} {name}";

            if (title.Length >= 32) // truncate if too long, used in GUI
                title = title[..29] + "...";

            // hex value preferable (but not consistent.. yet?)
            return $"[0x{VendorID:x4} 0x{ProductID:x4}]{Environment.NewLine}{title}";
        }
    }
}
