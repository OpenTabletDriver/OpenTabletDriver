using System.Linq;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.SystemDrivers.Providers
{
    internal class TabletDriverInfoProvider : IDriverInfoProvider
    {
        private static readonly string[] ProcessNames = new string[]
        {
            "TabletDriverGUI",
            "TabletDriverService"
        };

        public DriverInfo? GetDriverInfo()
        {
            if (SystemInterop.CurrentPlatform == SystemPlatform.Windows)
            {
                var processes = DriverInfo.SystemProcesses.Where(p => ProcessNames.Contains(p.ProcessName)).ToArray();
                if (processes.Any())
                {
                    return new DriverInfo
                    {
                        Name = "TabletDriver",
                        Processes = processes,
                        Status = DriverStatus.Active | DriverStatus.Blocking // TabletDriver opens tablets in exclusive mode by default
                    };
                }
            }

            return null;
        }
    }
}
