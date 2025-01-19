using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTabletDriver.Plugin.Devices
{
    public interface IDeviceEndpoint
    {
        int ProductID { get; }
        int VendorID { get; }
        int InputReportLength { get; }
        int OutputReportLength { get; }
        int FeatureReportLength { get; }

        string? Manufacturer { get; }
        string? ProductName { get; }
        string? FriendlyName { get; }
        string? SerialNumber { get; }

        string? DevicePath { get; }
        bool CanOpen { get; }
        IDictionary<string, string>? DeviceAttributes { get; }

        IDeviceEndpointStream Open();

        string GetDeviceString(byte index);
    }
}
