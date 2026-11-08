using System;
using System.Linq;

namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class TabletDriverInfoProvider : IDriverInfoProvider
    {
        private readonly string[] ProcessNames =
        [
            "TabletDriverGUI",
            "TabletDriverService",
        ];

        public DriverInfo? GetDriverInfo()
        {
            if (OperatingSystem.IsWindows())
            {
                var processes = DriverInfo.SystemProcesses.Where(p => ProcessNames.Contains(p.ProcessName)).ToArray();
                if (processes.Any())
                {
                    return new DriverInfo
                    {
                        Name = "TabletDriver",
                        Processes = [.. processes],
                        Status = DriverStatus.Active | DriverStatus.Blocking // TabletDriver opens tablets in exclusive mode by default
                    };
                }
            }

            return null;
        }
    }
}
