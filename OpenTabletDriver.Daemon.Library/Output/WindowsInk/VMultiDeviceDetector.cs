using System;
using System.Linq;
using HidSharp;
using HidSharp.Reports;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    internal static class VMultiDeviceDetector
    {
        public const int VendorId = 0x00ff;
        public const int ProductId = 0xbacc;
        public const string DownloadUrl = "https://github.com/X9VoiD/vmulti-bin/releases/latest";

        public static VMultiDeviceStatusDto GetStatus()
        {
            try
            {
                var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: ProductId).ToArray();
                return GetStatusFromDevices(devices, null, false);
            }
            catch (Exception ex)
            {
                return CreateStatus(
                    VMultiDeviceStatusKind.OpenFailed,
                    false,
                    false,
                    $"VMulti status could not be read: {ex.Message}"
                );
            }
        }

        public static bool TryOpenOutputDevice(out HidStream? stream, out VMultiDeviceStatusDto status)
        {
            stream = null;

            try
            {
                var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: ProductId).ToArray();

                foreach (var device in devices)
                {
                    if (IsOutputEndpoint(device) && device.TryOpen(out var openedStream))
                    {
                        stream = openedStream;
                        break;
                    }
                }

                status = GetStatusFromDevices(devices, stream, true);
                if (!status.IsAvailable)
                {
                    stream?.Dispose();
                    stream = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                stream?.Dispose();
                stream = null;
                status = CreateStatus(
                    VMultiDeviceStatusKind.OpenFailed,
                    false,
                    false,
                    $"VMulti status could not be read: {ex.Message}"
                );
                return false;
            }
        }

        private static VMultiDeviceStatusDto GetStatusFromDevices(HidDevice[] devices, HidStream? openedOutputStream, bool requireOpenOutputStream)
        {
            if (devices.Length == 0)
            {
                return CreateStatus(
                    VMultiDeviceStatusKind.Missing,
                    false,
                    false,
                    "VMulti VirtualHID was not found. Download VMulti, extract it, run install_hiddriver.bat as administrator, then restart OpenTabletDriver."
                );
            }

            var normalDigitizerReportAvailable = false;
            var extendedDigitizerReportAvailable = false;
            var outputDeviceAvailable = false;

            foreach (var device in devices)
            {
                outputDeviceAvailable |= IsOutputEndpoint(device);

                if (device.GetMaxInputReportLength() != 10)
                    continue;

                var reportDescriptor = device.GetReportDescriptor();
                if (reportDescriptor.TryGetReport(ReportType.Input, DigitizerInputReport.NormalReportId, out _))
                    normalDigitizerReportAvailable = true;
                if (reportDescriptor.TryGetReport(ReportType.Input, DigitizerInputReport.ExtendedReportId, out _))
                    extendedDigitizerReportAvailable = true;
            }

            if (!outputDeviceAvailable || (!normalDigitizerReportAvailable && !extendedDigitizerReportAvailable))
            {
                return CreateStatus(
                    VMultiDeviceStatusKind.Incomplete,
                    false,
                    extendedDigitizerReportAvailable,
                    "VMulti was found, but its HID endpoints or digitizer reports are incomplete. Reinstall VMulti as administrator, then restart OpenTabletDriver."
                );
            }

            if (requireOpenOutputStream && openedOutputStream == null)
            {
                return CreateStatus(
                    VMultiDeviceStatusKind.OpenFailed,
                    false,
                    extendedDigitizerReportAvailable,
                    "VMulti was found, but OpenTabletDriver could not open the VirtualHID output endpoint. Restart OpenTabletDriver or reinstall VMulti as administrator."
                );
            }

            return CreateStatus(
                VMultiDeviceStatusKind.Ready,
                true,
                extendedDigitizerReportAvailable,
                extendedDigitizerReportAvailable
                    ? "VMulti is installed and ready. Extended Windows Ink digitizer reports are available."
                    : "VMulti is installed and ready. Standard Windows Ink digitizer reports are available."
            );
        }

        private static bool IsOutputEndpoint(HidDevice device)
        {
            return device.GetMaxOutputReportLength() == 65 && device.GetMaxInputReportLength() == 65;
        }

        private static VMultiDeviceStatusDto CreateStatus(
            VMultiDeviceStatusKind kind,
            bool isAvailable,
            bool isExtendedDigitizerAvailable,
            string message
        )
        {
            return new VMultiDeviceStatusDto(kind, isAvailable, isExtendedDigitizerAvailable, message, DownloadUrl);
        }
    }
}
