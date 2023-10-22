namespace OpenTabletDriver.SystemDrivers.Providers
{
    internal class OpenTabletDriverInfoProvider : IDriverInfoProvider
    {
        public DriverInfo? GetDriverInfo()
        {
            var daemonInstanceName = "OpenTabletDriver.Daemon";
            if (Instance.Exists(daemonInstanceName) && !Instance.IsOwnerOf(daemonInstanceName))
            {
                return new DriverInfo
                {
                    Name = "OpenTabletDriver",
                    Status = DriverStatus.Active
                };
            }

            return null;
        }
    }
}
