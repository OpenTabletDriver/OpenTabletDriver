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

        [JsonProperty(nameof(DevicePath))]
        public required string DevicePath { get; set; }

        [JsonProperty(nameof(Manufacturer))]
        public string? Manufacturer { get; set; }

        [JsonProperty(nameof(ProductName))]
        public string? ProductName { get; set; }

        [JsonProperty(nameof(SerialNumber))]
        public string? SerialNumber { get; set; }

        [JsonProperty(nameof(FriendlyName))]
        public string? FriendlyName { get; set; }

        public int VendorID { get; set; }

        public int ProductID { get; set; }

        public int InputReportLength { get; set; }

        public int OutputReportLength { get; set; }

        public int FeatureReportLength { get; set; }

        public bool CanOpen { get; set; }

        public IDictionary<string, string> DeviceAttributes { get; set; }
    }
}
