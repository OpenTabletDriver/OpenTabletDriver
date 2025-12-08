using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HidSharp;
using HidSharp.Reports;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
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
        public IDictionary<string, string> DeviceAttributes => GetDeviceAttributes(DevicePath, () => device.GetReportDescriptor());

        public IDeviceEndpointStream Open() => device.TryOpen(out var stream) ? new HidSharpEndpointStream(stream) : null;
        public string GetDeviceString(byte index) => device.GetDeviceString(index);

        private static IDictionary<string, string> GetDeviceAttributes(string devicePath, Func<ReportDescriptor> reportDescriptorFunc)
        {
            var deviceAttributes = new Dictionary<string, string>();
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    GetDeviceAttributesWindows(devicePath, deviceAttributes);
                    break;
                case PluginPlatform.Linux:
                    GetDeviceAttributesLinux(devicePath, deviceAttributes);
                    break;
                case PluginPlatform.MacOS:
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
            GetInterfaceNumberFromPath(deviceAttributes, devicePath, @"^.*\/.*?:.*?\.(?<interface>\d+)\/.*?\/hidraw\/hidraw\d+$");
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
