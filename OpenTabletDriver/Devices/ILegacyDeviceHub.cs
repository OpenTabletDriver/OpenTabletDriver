namespace OpenTabletDriver.Devices
{
    public interface ILegacyDeviceHub
    {
        public bool TryGetDevice(string path, out IDeviceEndpoint endpoint);

        public bool CanEnumeratePorts { get; }

        public string[] EnumeratePorts();
    }
}
