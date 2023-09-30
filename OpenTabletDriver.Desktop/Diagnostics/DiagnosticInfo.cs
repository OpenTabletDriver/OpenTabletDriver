using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class DiagnosticInfo : IDiagnosticInfo
    {
        public DiagnosticInfo(
            IEnumerable<LogMessage> log,
            IEnumerable<IDeviceEndpoint> devices,
            EnvironmentDictionary environmentDictionary
        )
        {
            ConsoleLog = log;
            Devices = devices;
            EnvironmentVariables = environmentDictionary;
        }

        public string AppVersion { get; } = GetAppVersion();
        public string BuildDate { get; } = typeof(BuildDateAttribute).Assembly.GetCustomAttribute<BuildDateAttribute>()?.BuildDate ?? string.Empty;
        public IDictionary<string, string> OperatingSystem
        {
            get
            {
                switch (SystemInterop.CurrentPlatform)
                {
                    case SystemPlatform.Linux:
                        return LinuxEtcOSRelease.Raw;
                    default:
                        return OSVersionToDict();
                }
            }
        }
        public IDictionary<string, string> EnvironmentVariables { get; }
        public IEnumerable<IDeviceEndpoint> Devices { get; }
        public IEnumerable<LogMessage> ConsoleLog { get; }

        // behaves similarly to if Environment.OSVersion is passed directly to our serializer
        private static IDictionary<string,string> OSVersionToDict()
        {
            IDictionary<string, string> rv = new Dictionary<string, string>();
            rv.Add(nameof(Environment.OSVersion.Platform), Environment.OSVersion.Platform.ToString());
            rv.Add(nameof(Environment.OSVersion.ServicePack), Environment.OSVersion.ServicePack);
            rv.Add(nameof(Environment.OSVersion.Version), Environment.OSVersion.Version.ToString());
            rv.Add(nameof(Environment.OSVersion.VersionString), Environment.OSVersion.VersionString);
            return rv;
        }

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
