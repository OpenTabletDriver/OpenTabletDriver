namespace OpenTabletDriver.Devices
{
    public static class DeviceEndpointExtensions
    {
        public static string GetDetailedName(this IDeviceEndpoint endpoint)
        {
            return endpoint.FriendlyName != null
                ? $"{endpoint.FriendlyName} ({endpoint.DevicePath})"
                : endpoint.DevicePath;
        }
    }
}
