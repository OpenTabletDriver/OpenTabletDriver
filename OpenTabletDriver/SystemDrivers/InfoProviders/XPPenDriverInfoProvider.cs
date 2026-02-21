using System.Linq;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal partial class XPPenDriverInfoProvider : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "XP-Pen";

        protected override string LinuxFriendlyName => "UC Logic";

        protected override string LinuxModuleName => "hid_uclogic";

        protected override string[] WinProcessNames =>
        [
            "PentabletService",
            "PentabletUIService",
            "PenTablet",
        ];

        protected override string[] Heuristics { get; } =
        [
            "XP[ _-]*Pen",
            "Pentablet",
        ];

        private string[] Exclusions =
        [
            "Huion",
            "Gaomon",
            "Veikk",
        ];

        protected override DriverInfo? GetWinDriverInfo()
        {
            var processes = DriverInfo.SystemProcesses
                .Where(p => WinProcessNames.Concat(Heuristics)
                .Any(n => Regex.IsMatch(p.ProcessName, n, RegexOptions.IgnoreCase)) && !OpenTabletDriverRegex().IsMatch(p.ProcessName));

            var falsePositive = processes.Any(p => Exclusions.Any(ex => Regex.IsMatch(p.ProcessName, ex))) ? DriverStatus.Uncertain : 0;

            if (processes.Any())
            {
                return new DriverInfo
                {
                    Name = FriendlyName,
                    Processes = processes.ToArray(),
                    Status = DriverStatus.Active | falsePositive
                };
            }
            else
            {
                return null;
            }
        }

        [GeneratedRegex("OpenTabletDriver", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex OpenTabletDriverRegex();
    }
}
