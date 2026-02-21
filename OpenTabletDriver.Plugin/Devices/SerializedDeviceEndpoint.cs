using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Devices
{
    public class SerializedDeviceEndpoint
    {
        [JsonConstructor]
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
        public required string Manufacturer { get; set; }

        [JsonProperty(nameof(ProductName))]
        public required string ProductName { get; set; }

        [JsonProperty(nameof(SerialNumber))]
        public required string SerialNumber { get; set; }

        [JsonProperty(nameof(FriendlyName))]
        public required string FriendlyName { get; set; }

        [JsonProperty(nameof(VendorID))]
        public int VendorID { get; set; }

        [JsonProperty(nameof(ProductID))]
        public int ProductID { get; set; }

        [JsonProperty(nameof(InputReportLength))]
        public int InputReportLength { get; set; }

        [JsonProperty(nameof(OutputReportLength))]
        public int OutputReportLength { get; set; }

        [JsonProperty(nameof(FeatureReportLength))]
        public int FeatureReportLength { get; set; }

        [JsonProperty(nameof(CanOpen))]
        public bool CanOpen { get; set; }

        [JsonProperty(nameof(DeviceAttributes))]
        public IDictionary<string, string> DeviceAttributes { get; set; }
    }
}
