using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HidSharp;
using HidSharp.Reports;
using OpenTabletDriver.Interop;

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
        public IDictionary<string, string>? DeviceAttributes => GetDeviceAttributes(DevicePath, () => _device.GetReportDescriptor());

        public IDeviceEndpointStream? Open() => _device.TryOpen(out var stream) ? new HidSharpEndpointStream(stream) : null;
        public string GetDeviceString(byte index) => _device.GetDeviceString(index);

        private static IDictionary<string, string>? GetDeviceAttributes(string devicePath, Func<ReportDescriptor> reportDescriptorFunc)
        {
            var deviceAttributes = new Dictionary<string, string>();

            // - USB_INTERFACE_NUMBER
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    GetDeviceAttributesWindows(devicePath, deviceAttributes);
                    break;
                case SystemPlatform.Linux:
                    GetDeviceAttributesLinux(devicePath, deviceAttributes);
                    break;
                case SystemPlatform.MacOS:
                    GetDeviceAttributesMacOS(devicePath, deviceAttributes);
                    break;
            }

            Extensions.ExtractHidUsages(deviceAttributes, reportDescriptorFunc);

            return deviceAttributes;
        }

        private static void GetDeviceAttributesWindows(string devicePath, Dictionary<string, string> deviceAttributes)
        {
            GetInterfaceNumberFromPath(deviceAttributes, devicePath, @"&mi_(?<interface>\d+)");
        }

        private static void GetDeviceAttributesLinux(string devicePath, Dictionary<string, string> deviceAttributes)
        {
            GetInterfaceNumberFromPath(deviceAttributes, devicePath, @"^.*\/.*?:.*?\.(?<interface>.+)\/.*?\/hidraw\/hidraw\d+$");
        }

        private static void GetDeviceAttributesMacOS(string devicePath, Dictionary<string, string> deviceAttributes)
        {
            GetInterfaceNumberFromPath(deviceAttributes, devicePath, @"IOUSBHostInterface@(?<interface>\d+)");
        }

        private static void GetInterfaceNumberFromPath(Dictionary<string, string> attributes, string path, string regex)
        {
            var match = Regex.Match(path, regex);
            if (!match.Success)
                return;

            var interfaceNumber = int.Parse(match.Groups["interface"].Value);
            attributes.Add("USB_INTERFACE_NUMBER", interfaceNumber.ToString());
        }
    }
}
