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

        [JsonProperty(nameof(DevicePath), Order = 1000)]
        public required string DevicePath { get; set; }

        [JsonProperty(nameof(Manufacturer))]
        public string? Manufacturer { get; set; }

        [JsonProperty(nameof(ProductName))]
        public string? ProductName { get; set; }

        [JsonProperty(nameof(SerialNumber))]
        public string? SerialNumber { get; set; }

        [JsonProperty(nameof(FriendlyName))]
        public string? FriendlyName { get; set; }

        [JsonProperty(Order = -100)]
        public int VendorID { get; set; }

        [JsonProperty(Order = -90)]
        public int ProductID { get; set; }

        [JsonProperty(Order = -80)]
        public int InputReportLength { get; set; }

        [JsonProperty(Order = -70)]
        public int OutputReportLength { get; set; }

        [JsonProperty(Order = -60)]
        public int FeatureReportLength { get; set; }

        [JsonProperty(Order = -50)]
        public bool CanOpen { get; set; }

        [JsonProperty(nameof(DeviceAttributes), Order = -40)]
        public IDictionary<string, string> DeviceAttributes { get; set; }
    }
}
