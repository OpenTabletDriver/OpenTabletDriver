using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using OpenTabletDriver.SystemDrivers.Providers;

namespace OpenTabletDriver.SystemDrivers
{
    /// <summary>
    /// Contains information and hints about an installed tablet driver.
    /// </summary>
    /// <remarks>
    /// See <see cref="GetDriverInfos"/> to get all the currently active tablet drivers.
    /// </remarks>
    [PublicAPI]
    public class DriverInfo
    {
        /// <summary>
        /// The human-friendly name of the driver.
        /// </summary>
        public string Name { internal set; get; } = string.Empty;

        /// <summary>
        /// Running processes that might be associated with the driver.
        /// </summary>
        /// <remarks>
        /// This is set to null when there is no associated process.
        /// </remarks>
        public Process[] Processes { internal set; get; } = Array.Empty<Process>();

        /// <summary>
        /// Tells how this driver is currently affecting OpenTabletDriver's operations.
        /// </summary>
        public DriverStatus Status { internal set; get; }

        /// <summary>
        /// Retrieves all the currently active tablet drivers.
        /// </summary>
        public static IEnumerable<DriverInfo> GetDriverInfos()
        {
            var providers = new IDriverInfoProvider[]
            {
                new WacomDriverInfoProvider(),
                new GaomonDriverInfoProvider(),
                new HuionDriverInfoProvider(),
                new XPPenDriverInfoProvider(),
                new VeikkDriverInfoDriver(),
                new OpenTabletDriverInfoProvider(),
                new TabletDriverInfoProvider()
            };

            SystemProcesses = Process.GetProcesses();
            ProcessModuleQueryableDriverInfoProvider.Refresh();

            // Remove "UC Logic" duplicates
            return providers.Select(provider => provider.GetDriverInfo())
                .Where(i => i != null)
                .GroupBy(i => i!.Name)
                .Select(g => g.First())!;
        }

        internal static Process[] SystemProcesses { get; private set; } = Array.Empty<Process>();
    }
}
