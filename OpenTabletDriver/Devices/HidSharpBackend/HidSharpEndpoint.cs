using HidSharp;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    internal sealed class HidSharpEndpoint : IDeviceEndpoint
    {
        public HidSharpEndpoint(HidDevice device)
        {
            _device = device;
        }

        private readonly HidDevice _device;

        public int ProductID => _device.ProductID;
        public int VendorID => _device.VendorID;
        public int InputReportLength => _device.SafeGet(d => d.GetMaxInputReportLength(), -1);
        public int OutputReportLength => _device.SafeGet(d => d.GetMaxOutputReportLength(), -1);
        public int FeatureReportLength => _device.SafeGet(d => d.GetMaxFeatureReportLength(), -1);

        public string Manufacturer => _device.SafeGet(d => d.GetManufacturer(), "Unknown Manufacturer");
        public string ProductName => _device.SafeGet(d => d.GetProductName(), "Unknown Product Name");
        public string FriendlyName => _device.SafeGet(d => d.GetFriendlyName(), "Unknown Product Name");
        public string? SerialNumber => _device.SafeGet(d => d.GetSerialNumber(), null);
        public string DevicePath => _device.SafeGet(d => d.DevicePath, "Invalid Device Path");
        public bool CanOpen => _device.SafeGet(d => d.CanOpen, false);

        public IDeviceEndpointStream? Open() => _device.TryOpen(out var stream) ? new HidSharpEndpointStream(stream) : null;
        public string GetDeviceString(byte index) => _device.GetDeviceString(index);

        public bool IsSibling(IDeviceEndpoint other)
        {
            if (other is HidSharpEndpoint endpoint)
                return _device.IsSibling(endpoint._device);

            return false;
        }
    }
}
