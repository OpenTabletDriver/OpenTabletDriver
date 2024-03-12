using System.Collections.Generic;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableDeviceEndpoint : Serializable, IDeviceEndpoint
    {
        public int VendorID { set; get; }
        public int ProductID { set; get; }
        public int InputReportLength { set; get; }
        public int OutputReportLength { set; get; }
        public int FeatureReportLength { set; get; }
        public string Manufacturer { set; get; } = string.Empty;
        public string ProductName { set; get; } = string.Empty;
        public string FriendlyName { set; get; } = string.Empty;
        public string SerialNumber { set; get; } = string.Empty;
        public string DevicePath { set; get; } = string.Empty;
        public bool CanOpen { set; get; }
        public IDictionary<string, string>? DeviceAttributes { set; get; }

        public IDeviceEndpointStream Open() => throw NotSupported();
        public string GetDeviceString(byte index) => throw NotSupported();
    }
}
