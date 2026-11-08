using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.SystemDrivers.InfoProviders
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
            return true switch
            {
                _ when OperatingSystem.IsWindows() => GetWinDriverInfo(),
                _ when OperatingSystem.IsLinux() => GetLinuxDriverInfo(),
                _ => null
            };
        }

        [SupportedOSPlatform("windows")]
        protected virtual DriverInfo? GetWinDriverInfo()
        {
            if (pnpUtil == null) throw new InvalidOperationException($"pnpUtil is null. Run {nameof(Refresh)}() first");

            IEnumerable<Process> processes;
            var match = Heuristics.Any(name => Regex.IsMatch(pnpUtil, name, RegexOptions.IgnoreCase));
            if (match)
            {
                processes = DriverInfo.SystemProcesses
                    .Where(p => WinProcessNames.Concat(Heuristics)
                    .Any(n => Regex.IsMatch(p.ProcessName, n, RegexOptions.IgnoreCase)));

                var status = DriverStatus.Blocking;
                if (processes.Any())
                    status |= DriverStatus.Active;

                return new DriverInfo
                {
                    Name = FriendlyName,
                    Processes = processes.Any() ? processes.ToArray() : Array.Empty<Process>(),
                    Status = status
                };
            }

            return null;
        }

        [SupportedOSPlatform("linux")]
        protected virtual DriverInfo? GetLinuxDriverInfo()
        {
            // short circuit if modules aren't filled
            if (string.IsNullOrEmpty(linuxModules))
                return null;

            if (Regex.IsMatch(linuxModules, LinuxModuleName))
            {
                return new DriverInfo
                {
                    Name = LinuxFriendlyName,
                    Processes = [],
                    Status = DriverStatus.Active | DriverStatus.Blocking
                };
            }
            else
            {
                return null;
            }
        }

        internal static void Refresh()
        {
            if (OperatingSystem.IsWindows())
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
            }
            else if (OperatingSystem.IsLinux())
            {
                if (File.Exists("/proc/modules"))
                {
                    linuxModules = File.ReadAllText("/proc/modules");
                }
                else
                {
                    // TODO: check /proc/config.gz for the following keys defined with the value 'y' or 'm'
                    // |   module name   |   config.gz key   |
                    // | ------------- | ------------------- |
                    // |   hid_uclogic   | CONFIG_HID_UCLOGIC  |
                    // |    wacom      |  CONFIG_HID_WACOM   |

                    Log.Write(nameof(ProcessModuleQueryableDriverInfoProvider),
                        "Unable to probe kernel modules. Foreign driver detection may be unavailable.");
                }
            }
        }
    }
}
