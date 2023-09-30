using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class DiagnosticInfo
    {
        public DiagnosticInfo(IEnumerable<LogMessage> log, IEnumerable<SerializedDeviceEndpoint> devices)
        {
            ConsoleLog = log;
            Devices = devices;
        }

        [JsonProperty("App Version")]
        public string AppVersion { private set; get; } = GetAppVersion();

        [JsonProperty("Build Date")]
        public string BuildDate { private set; get; } = typeof(BuildDateAttribute).Assembly.GetCustomAttribute<BuildDateAttribute>().BuildDate;

        [JsonProperty("Operating System")]
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

        [JsonProperty("Environment Variables")]
        public IDictionary<string, string> EnvironmentVariables { private set; get; } = new EnvironmentDictionary();

        [JsonProperty("HID Devices")]
        public IEnumerable<SerializedDeviceEndpoint> Devices { private set; get; }

        [JsonProperty("Console Log")]
        public IEnumerable<LogMessage> ConsoleLog { private set; get; }

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
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            return $"OpenTabletDriver v{version}";
        }

        [OnError]
        internal void OnError(StreamingContext _, ErrorContext errorContext)
        {
            errorContext.Handled = true;
            Log.Write("Diagnostics", $"Handled diagnostics serialization error", LogLevel.Error);
            Log.Exception(errorContext.Error);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
