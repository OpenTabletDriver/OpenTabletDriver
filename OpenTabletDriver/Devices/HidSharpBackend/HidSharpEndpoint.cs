using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HidSharp;
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
        public IDictionary<string, string> DeviceAttributes => GetDeviceAttributes();

        public IDeviceEndpointStream Open() => device.TryOpen(out var stream) ? new HidSharpEndpointStream(stream) : null;
        public string GetDeviceString(byte index) => device.GetDeviceString(index);

        // - HID_REPORTS (report_id:usage_page:usage_id, ...)
        // - USB_INTERFACE_NUMBER
        private IDictionary<string, string> GetDeviceAttributes()
        {
            var deviceAttributes = new Dictionary<string, string>();
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    GetDeviceAttributesWindows(deviceAttributes);
                    break;
                case PluginPlatform.Linux:
                    GetDeviceAttributesLinux(deviceAttributes);
                    break;
                case PluginPlatform.MacOS:
                    GetDeviceAttributesMacOS(deviceAttributes);
                    break;
            }

            try
            {
                ExtractHidUsages(deviceAttributes);
            }
            catch
            {
                deviceAttributes.Add("HID_REPORT_DESCRIPTOR_NON_RECONSTRUCTABLE", "true");
            }

            return deviceAttributes;
        }

        private void ExtractHidUsages(Dictionary<string, string> deviceAttributes)
        {
            var reportDescriptor = device.GetReportDescriptor();

            List<(byte, uint)> usages = new List<(byte, uint)>();
            foreach (var inputReport in reportDescriptor.InputReports)
            {
                var reportId = inputReport.ReportID;
                usages.AddRange(inputReport.DeviceItem.Usages.GetAllValues().Select(x => (reportId, x)));
            }

            var hidReportsBuilder = new StringBuilder();
            var enumerator = usages.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var reportId = enumerator.Current.Item1;
                var extendedUsage = enumerator.Current.Item2;
                appendHidReport(hidReportsBuilder, reportId, extendedUsage);
                while (enumerator.MoveNext())
                {
                    hidReportsBuilder.Append(", ");
                    reportId = enumerator.Current.Item1;
                    extendedUsage = enumerator.Current.Item2;
                    appendHidReport(hidReportsBuilder, reportId, extendedUsage);
                }

                static void appendHidReport(StringBuilder stringBuilder, byte reportId, uint extendedUsage)
                {
                    var usagePage = (extendedUsage & 0xffff0000) >> 16;
                    var usageId = extendedUsage & 0x0000ffff;
                    stringBuilder.Append($"{reportId:X2}:{usagePage:X4}:{usageId:X4}");
                }
            }

            deviceAttributes.Add("HID_REPORTS", hidReportsBuilder.ToString());
        }

        private void GetDeviceAttributesWindows(Dictionary<string, string> deviceAttributes)
        {
            GetInterfaceNumberFromPath(deviceAttributes, DevicePath, @"&mi_(?<interface>\d+)");
        }

        private void GetDeviceAttributesLinux(Dictionary<string, string> deviceAttributes)
        {
            GetInterfaceNumberFromPath(deviceAttributes, DevicePath, @"^.*\/.*?:.*?\.(?<interface>.+)\/.*?\/hidraw\/hidraw\d+$");
        }

        private void GetDeviceAttributesMacOS(Dictionary<string, string> deviceAttributes)
        {
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
