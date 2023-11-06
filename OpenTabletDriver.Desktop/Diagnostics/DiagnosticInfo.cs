using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class DiagnosticInfo : IDiagnosticInfo
    {
        public DiagnosticInfo(
            IEnumerable<LogMessage> log,
            IEnumerable<IDeviceEndpoint> devices,
            EnvironmentDictionary environmentDictionary,
            Settings settings
        )
        {
            ConsoleLog = log;
            Devices = devices;
            EnvironmentVariables = environmentDictionary;
            ToolSettings = settings.Tools;
            ProfileSettings = settings.Profiles;
        }

        public string AppVersion { get; } = GetAppVersion();
        public string BuildDate { get; } = typeof(BuildDateAttribute).Assembly.GetCustomAttribute<BuildDateAttribute>()?.BuildDate ?? string.Empty;
        public OSInfo OperatingSystem { get; } = OSInfo.GetOSInfo();
        public IDictionary<string, string> EnvironmentVariables { get; }
        public IEnumerable<IDeviceEndpoint> Devices { get; }
        public IEnumerable<PluginSettings> ToolSettings { get; }
        public IEnumerable<Profile> ProfileSettings { get; }
        public IEnumerable<LogMessage> ConsoleLog { get; }

        private static string GetAppVersion()
        {
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            return $"OpenTabletDriver v{version}";
        }

        public override string ToString()
        {
            using (var memoryStream = new MemoryStream())
            using (var sr = new StreamReader(memoryStream))
            {
                Serialization.Serialize(memoryStream, this);
                return sr.ReadToEnd();
            }
        }
    }
}
