using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenTabletDriver.Plugin.Devices
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SerializedDeviceEndpoint
    {
        public SerializedDeviceEndpoint()
        {
        }

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

        public string? DevicePath { get; set; }

        public string? Manufacturer { get; set; }

        public string? ProductName { get; set; }

        public string? SerialNumber { get; set; }

        public string? FriendlyName { get; set; }

        public int VendorID { get; set; }

        public int ProductID { get; set; }

        public int? InputReportLength { get; set; }

        public int? OutputReportLength { get; set; }

        public int? FeatureReportLength { get; set; }

        public bool CanOpen { get; set; }

        public IDictionary<string, string>? DeviceAttributes { get; set; }
    }
}
