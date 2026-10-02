using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using HidSharp;
using HidSharp.Reports;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public class HidSharpEndpoint : IDeviceEndpoint
    {
        private const string HID_READER_THREAD_NAME = "HID Reader";
        private const int HID_READER_TIMEOUT_MS = 100;

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

        public IDeviceEndpointStream Open()
        {
            var existingThreadIds = SystemInterop.CurrentPlatform == PluginPlatform.MacOS ? Mach.GetThreadIds() : null;

            if (!device.TryOpen(out var stream))
                throw new InvalidOperationException("Unable to open device stream");

            if (existingThreadIds != null)
                SetMacOSRealtimePolicyOnReaderThread(existingThreadIds);

            return new HidSharpEndpointStream(stream);
        }

        public string GetDeviceString(byte index) => device.GetDeviceString(index);

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

        // Same policy as DeviceReader's thread. HidSharp offers no hook into its reader thread, so it is found by name
        // among the threads TryOpen started. macOS only lets a thread name itself (see pthread_setname_np(3)), so the
        // name can appear after TryOpen returns: up to ~10 ms under load, well within HID_READER_TIMEOUT_MS.
        // TODO: Set the policy at the start of HIDSharpCore's MacHidStream.ReadThread instead, and drop this lookup.
        private static void SetMacOSRealtimePolicyOnReaderThread(IReadOnlySet<ulong> existingThreadIds)
        {
            var stopwatch = Stopwatch.StartNew();
            int result;
            while (!Mach.TrySetNewNamedThreadTimeConstraint(
                HID_READER_THREAD_NAME,
                existingThreadIds,
                computation: TimeSpan.FromMilliseconds(1),
                constraint: TimeSpan.FromMilliseconds(2),
                out result
            ))
            {
                if (stopwatch.ElapsedMilliseconds > HID_READER_TIMEOUT_MS)
                {
                    Log.Write("Device", $"Failed to set real-time thread policy: no new '{HID_READER_THREAD_NAME}' thread from HidSharp", LogLevel.Warning);
                    return;
                }
                Thread.Sleep(1);
            }

            if (result != 0)
                Log.Write("Device", $"Failed to set real-time thread policy on HidSharp's '{HID_READER_THREAD_NAME}' thread: kern_return {result}", LogLevel.Warning);
        }
    }
}
