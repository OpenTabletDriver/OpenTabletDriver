using System;

namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class WacomDriverInfoProvider : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "Wacom";

        protected override string LinuxFriendlyName => FriendlyName;

        protected override string LinuxModuleName => "wacom";

        protected override string[] WinProcessNames { get; } = Array.Empty<string>();

        protected override string[] Heuristics { get; } = new string[]
        {
            "Wacom"
        };

        protected override DriverInfo GetWinDriverInfo()
        {
            var info = base.GetWinDriverInfo();
            if (info != null)
                info.IsBlockingDriver = false;

            return info;
        }

        protected override DriverInfo GetLinuxDriverInfo()
        {
            var info = base.GetLinuxDriverInfo();
            if (info != null)
                info.IsBlockingDriver = false;

            return info;
        }
    }
}
