using System.Linq;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class TabletDriverInfoProvider : IDriverInfoProvider
    {
        private readonly string[] ProcessNames = new string[]
        {
            "TabletDriverGUI",
            "TabletDriverService"
        };

        public DriverInfo GetDriverInfo()
        {
            if (SystemInterop.CurrentPlatform == PluginPlatform.Windows)
            {
                var processes = DriverInfo.SystemProcesses.Where(p => ProcessNames.Contains(p.ProcessName)).ToArray();
                if (processes.Any())
                {
                    return new DriverInfo
                    {
                        Name = "TabletDriver",
                        Processes = processes,
                        IsBlockingDriver = true, // TabletDriver opens tablets in exclusive mode by default
                        IsSendingInput = true
                    };
                }
            }

            return null;
        }
    }
}
