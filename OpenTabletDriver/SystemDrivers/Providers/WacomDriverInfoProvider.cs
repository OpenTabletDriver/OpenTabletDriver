using System;

namespace OpenTabletDriver.SystemDrivers.Providers
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

        protected override DriverInfo? GetWinDriverInfo()
        {
            var info = base.GetWinDriverInfo();
            if (info != null)
            {
                // wacom drivers doesn't block access/detection.
                info.Status &= ~DriverStatus.Blocking;

                // wacom filter drivers causes feature reports to be blocked
                // which causes issues when wacom driver is not running and the
                // tablet has to be re-initialized using a feature report.
                info.Status |= DriverStatus.Flaky;
            }

            return info;
        }

        protected override DriverInfo? GetLinuxDriverInfo()
        {
            var info = base.GetLinuxDriverInfo();
            if (info != null)
            {
                // wacom drivers doesn't block access/detection
                info.Status &= ~DriverStatus.Blocking;
            }

            return info;
        }
    }
}
