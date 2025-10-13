namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    public class OpenTabletDriverInfoProvider : IDriverInfoProvider
    {
        public DriverInfo GetDriverInfo()
        {
            var daemonInstanceName = "OpenTabletDriver.Daemon";
            if (Instance.Exists(daemonInstanceName) && !Instance.IsOwnerOf(daemonInstanceName))
            {
                return new DriverInfo
                {
                    Name = "OpenTabletDriver",
                    Processes = [],
                    Status = DriverStatus.Active
                };
            }

            return null;
        }
    }
}
