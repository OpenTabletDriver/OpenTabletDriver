using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.SystemDrivers.Providers
{
    internal abstract class ProcessModuleQueryableDriverInfoProvider : IDriverInfoProvider
    {
        protected abstract string FriendlyName { get; }
        protected abstract string LinuxFriendlyName { get; }
        protected abstract string LinuxModuleName { get; }
        protected abstract string[] WinProcessNames { get; }
        protected abstract string[] Heuristics { get; }

        private static string? pnpUtil;
        private static string? linuxModules;

        public DriverInfo? GetDriverInfo()
        {
            return SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => GetWinDriverInfo(),
                SystemPlatform.Linux => GetLinuxDriverInfo(),
                _ => null
            };
        }

        protected virtual DriverInfo? GetWinDriverInfo()
        {
            if (pnpUtil == null)
                Refresh();

            var match = Heuristics.Any(name => Regex.IsMatch(pnpUtil!, name, RegexOptions.IgnoreCase));
            if (match)
            {
                var processes = DriverInfo.SystemProcesses
                    .Where(p => WinProcessNames.Concat(Heuristics)
                        .Any(n => Regex.IsMatch(p.ProcessName, n, RegexOptions.IgnoreCase)));

                return new DriverInfo
                {
                    Name = FriendlyName,
                    Processes = processes.Any() ? processes.ToArray() : Array.Empty<Process>(),
                    IsBlockingDriver = true,
                    IsSendingInput = processes.Any()
                };
            }

            return null;
        }

        protected virtual DriverInfo? GetLinuxDriverInfo()
        {
            if (linuxModules == null)
                Refresh();

            if (Regex.IsMatch(linuxModules!, LinuxModuleName))
            {
                return new DriverInfo
                {
                    Name = LinuxFriendlyName,
                    IsBlockingDriver = true,
                    IsSendingInput = true
                };
            }

            return null;
        }

        internal static void Refresh()
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                {
                    var pnputilProc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "pnputil.exe",
                            Arguments = "-e",
                            UseShellExecute = false,
                            RedirectStandardOutput = true
                        }
                    };

                    pnputilProc.Start();
                    pnpUtil = pnputilProc.StandardOutput.ReadToEnd();
                    break;
                }
                case SystemPlatform.Linux:
                {
                    linuxModules = File.ReadAllText("/proc/modules");
                    break;
                }
            }
        }
    }
}
