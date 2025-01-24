using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Platform.Environment;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class DiagnosticInfo
    {
        public string AppVersion { get; } = GetAppVersion();
        public string BuildDate { get; } = typeof(BuildDateAttribute).Assembly.GetCustomAttribute<BuildDateAttribute>()?.BuildDate ?? string.Empty;
        public OSInfo OSInfo { get; } = OSInfo.GetOSInfo();
        public IDictionary<string, string>? EnvironmentVariables { get; }
        public IEnumerable<DeviceEndpointDto>? Devices { get; }
        public IEnumerable<LogMessage>? LogMessages { get; }

        [JsonConstructor]
        public DiagnosticInfo(IDictionary<string, string>? environmentVariables, IEnumerable<DeviceEndpointDto>? devices,
            IEnumerable<LogMessage>? logMessages)
        {
            EnvironmentVariables = environmentVariables;
            Devices = devices;
            LogMessages = logMessages;
        }

        public DiagnosticInfo(IEnvironmentDictionary environmentDictionary, IDriverDaemon driverDaemon)
        {
            Devices = Task.Run(driverDaemon.GetDevices).Result;
            LogMessages = Task.Run(driverDaemon.GetCurrentLog).Result;
            EnvironmentVariables = environmentDictionary.Variables;
        }

        private static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            return $"OpenTabletDriver v{version}";
        }
    }
}
