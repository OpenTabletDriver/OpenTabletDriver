using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Tests.TabletMatching
{
    public class MockedDeviceEndpoint(SerializedDeviceEndpoint serializedDeviceEndpoint, IReadOnlyDictionary<byte, string> deviceStrings) : IDeviceEndpoint
    {
        public int ProductID { get; } = serializedDeviceEndpoint.ProductID;
        public int VendorID { get; } = serializedDeviceEndpoint.VendorID;
        public int InputReportLength { get; } = serializedDeviceEndpoint.InputReportLength!;
        public int OutputReportLength { get; } = serializedDeviceEndpoint.OutputReportLength!;
        public int FeatureReportLength { get; } = serializedDeviceEndpoint.FeatureReportLength!;
        public string? Manufacturer => serializedDeviceEndpoint.Manufacturer;
        public string? ProductName => serializedDeviceEndpoint.ProductName;
        public string? FriendlyName => serializedDeviceEndpoint.FriendlyName;
        public string? SerialNumber => serializedDeviceEndpoint.SerialNumber;
        public string DevicePath => serializedDeviceEndpoint.DevicePath;
        public bool CanOpen => serializedDeviceEndpoint.CanOpen;

        public IDictionary<string, string> DeviceAttributes { get; } =
            serializedDeviceEndpoint.DeviceAttributes;

        public IReadOnlyDictionary<byte, string> DeviceStrings { get; } = deviceStrings;

        public IDeviceEndpointStream Open()
        {
            // may be relevant to implement later
            throw new NotImplementedException();
        }

        public string GetDeviceString(byte index) =>
            DeviceStrings.TryGetValue(index, out string? result) ? result : string.Empty;

        public void OpenAndSendFeatureInit(byte[] initData) => throw new NotImplementedException();

        public void OpenAndSendOutputInit(byte[] initData) => throw new NotImplementedException();
    }
}
