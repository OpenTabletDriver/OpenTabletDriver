using HidSharp;
using OpenTabletDriver;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public class HidSharpEndpoint : IDeviceEndpoint
    {
        internal HidSharpEndpoint(HidDevice device)
        {
            this.device = device;
        }

        private HidDevice device;

        public int ProductID => device.ProductID;
        public int VendorID => device.VendorID;
        public int InputReportLength => device.SafeGet(d => d.GetMaxInputReportLength(), -1);
        public int OutputReportLength => device.SafeGet(d => d.GetMaxOutputReportLength(), -1);
        public int FeatureReportLength => device.SafeGet(d => d.GetMaxFeatureReportLength(), -1);

        public string Manufacturer => device.SafeGet(d => d.GetManufacturer(), "Unknown Manufacturer");
        public string ProductName => device.SafeGet(d => d.GetProductName(), "Unknown Product Name");
        public string FriendlyName => device.SafeGet(d => d.GetFriendlyName(), "Unknown Product Name");
        public string SerialNumber => device.SafeGet(d => d.GetSerialNumber(), string.Empty);
        public string DevicePath => device.SafeGet(d => d.DevicePath, "Invalid Device Path");
        public bool CanOpen => device.SafeGet(d => d.CanOpen, false);

        public IDeviceEndpointStream Open() => device.TryOpen(out var stream) ? new HidSharpEndpointStream(stream) : null;
        public string GetDeviceString(byte index) => device.GetDeviceString(index);
    }
}
