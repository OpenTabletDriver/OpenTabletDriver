using Newtonsoft.Json;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class DeviceEndpointDto
    {
        [JsonConstructor]
        public DeviceEndpointDto(
            int productID,
            int vendorID,
            int inputReportLength,
            int outputReportLength,
            int featureReportLength,
            string? manufacturer,
            string? productName,
            string? friendlyName,
            string? serialNumber,
            string devicePath,
            bool canOpen
        )
        {
            ProductID = productID;
            VendorID = vendorID;
            InputReportLength = inputReportLength;
            OutputReportLength = outputReportLength;
            FeatureReportLength = featureReportLength;
            Manufacturer = manufacturer;
            ProductName = productName;
            FriendlyName = friendlyName;
            SerialNumber = serialNumber;
            DevicePath = devicePath;
            CanOpen = canOpen;
        }

        public DeviceEndpointDto(IDeviceEndpoint endpoint)
        {
            ProductID = endpoint.ProductID;
            VendorID = endpoint.VendorID;
            InputReportLength = endpoint.InputReportLength;
            OutputReportLength = endpoint.OutputReportLength;
            FeatureReportLength = endpoint.FeatureReportLength;
            Manufacturer = endpoint.Manufacturer;
            ProductName = endpoint.ProductName;
            FriendlyName = endpoint.FriendlyName;
            SerialNumber = endpoint.SerialNumber;
            DevicePath = endpoint.DevicePath;
            CanOpen = endpoint.CanOpen;
        }

        public int ProductID { get; }
        public int VendorID { get; }
        public int InputReportLength { get; }
        public int OutputReportLength { get; }
        public int FeatureReportLength { get; }
        public string? Manufacturer { get; }
        public string? ProductName { get; }
        public string? FriendlyName { get; }
        public string? SerialNumber { get; }
        public string DevicePath { get; }
        public bool CanOpen { get; }
    }
}
