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
            : this(device, LinuxRawHidDescriptorInfo.TryCreateFromDevice)
        {
        }

        internal HidSharpEndpoint(HidDevice device, Func<string, LinuxRawHidDescriptorInfo> linuxFallbackInfoProvider)
        {
            this.device = device;
            this.linuxFallbackInfoProvider = linuxFallbackInfoProvider;
        }

        private HidDevice device;
        private readonly Func<string, LinuxRawHidDescriptorInfo> linuxFallbackInfoProvider;
        private bool linuxFallbackInfoInitialized;
        private LinuxRawHidDescriptorInfo linuxFallbackInfo;

        public int ProductID => device.ProductID;
        public int VendorID => device.VendorID;
        public int InputReportLength => GetReportLength(d => d.GetMaxInputReportLength(), i => i.InputReportLength);
        public int OutputReportLength => GetReportLength(d => d.GetMaxOutputReportLength(), i => i.OutputReportLength);
        public int FeatureReportLength => GetReportLength(d => d.GetMaxFeatureReportLength(), i => i.FeatureReportLength);

        public string Manufacturer => device.SafeGet(d => d.GetManufacturer(), "Unknown Manufacturer");
        public string ProductName => device.SafeGet(d => d.GetProductName(), "Unknown Product Name");
        public string FriendlyName => device.SafeGet(d => d.GetFriendlyName(), "Unknown Product Name");
        public string SerialNumber => device.SafeGet(d => d.GetSerialNumber(), string.Empty);
        public string DevicePath => device.SafeGet(d => d.DevicePath, "Invalid Device Path");
        public bool CanOpen => device.SafeGet(d => d.CanOpen, false);
        public IDictionary<string, string> DeviceAttributes => GetDeviceAttributes(DevicePath, () => device.GetReportDescriptor());

        public IDeviceEndpointStream Open()
        {
            if (device.TryOpen(out var stream))
                return new HidSharpEndpointStream(stream);

            // Fallback for devices with HID descriptors that HidSharp cannot parse
            // (e.g. UnitExponent encoded as a signed byte instead of a 4-bit nibble).
            // The kernel's hidraw interface exposes raw reports without requiring a
            // parseable descriptor, so we can still read from the device via file I/O.
            if (TryGetLinuxFallbackInfo(out var fallbackInfo))
            {
                try
                {
                    var fileSystemName = GetLinuxFileSystemName();
                    if (!string.IsNullOrWhiteSpace(fileSystemName))
                        return new LinuxRawHidStream(fileSystemName, fallbackInfo);
                }
                catch { }
            }

            return null;
        }
        public string GetDeviceString(byte index) => device.GetDeviceString(index);

        private int GetReportLength(Func<HidDevice, int> getter, Func<LinuxRawHidDescriptorInfo, int> fallbackSelector)
        {
            if (device.TryGet(getter, out var length))
                return length;

            return TryGetLinuxFallbackInfo(out var fallbackInfo)
                ? fallbackSelector(fallbackInfo)
                : -1;
        }

        private bool TryGetLinuxFallbackInfo(out LinuxRawHidDescriptorInfo fallbackInfo)
        {
            fallbackInfo = null;
            if (SystemInterop.CurrentPlatform != PluginPlatform.Linux)
                return false;

            if (!linuxFallbackInfoInitialized)
            {
                var fileSystemName = GetLinuxFileSystemName();
                linuxFallbackInfo = string.IsNullOrWhiteSpace(fileSystemName)
                    ? null
                    : linuxFallbackInfoProvider(fileSystemName);
                linuxFallbackInfoInitialized = true;
            }

            fallbackInfo = linuxFallbackInfo;
            return fallbackInfo != null;
        }

        private string GetLinuxFileSystemName() => device.SafeGet(d => d.GetFileSystemName(), null);

        private static Dictionary<string, string> GetDeviceAttributes(string devicePath, Func<ReportDescriptor> reportDescriptorFunc)
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
