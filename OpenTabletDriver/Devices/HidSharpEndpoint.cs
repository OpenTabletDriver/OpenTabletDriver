using OpenTabletDriver;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices
{
    public class HidSharpEndpoint : IDeviceEndpoint
    {
        internal HidSharpEndpoint(HidSharp.HidDevice device)
        {
            this.device = device;
        }

        private HidSharp.HidDevice device;

        public int ProductID => device.ProductID;
        public int VendorID => device.VendorID;
        public int InputReportLength => device.GetMaxInputReportLength();
        public int OutputReportLength => device.GetMaxOutputReportLength();
        public int FeatureReportLength => device.GetMaxFeatureReportLength();

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