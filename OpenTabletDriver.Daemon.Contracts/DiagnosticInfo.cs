using System.Collections.Generic;
using System.Reflection;
using OpenTabletDriver.Attributes;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class DiagnosticInfo
    {
        public string AppVersion { get; } = GetAppVersion();
        public string BuildDate { get; } = typeof(BuildDateAttribute).Assembly.GetCustomAttribute<BuildDateAttribute>()?.BuildDate ?? string.Empty;
        public OSInfo OSInfo { get; } = OSInfo.GetOSInfo();
        public IDictionary<string, string>? EnvironmentVariables { get; }
        public IEnumerable<DeviceEndpointDto>? Devices { get; }

        private static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            return $"OpenTabletDriver v{version}";
        }
    }
}
