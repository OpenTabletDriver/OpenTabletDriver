using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Attributes;

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
        public OperatingSystem OperatingSystem { private set; get; } = Environment.OSVersion;

        [JsonProperty("Environment Variables")]
        public IDictionary EnvironmentVariables { private set; get; } = Environment.GetEnvironmentVariables();

        [JsonProperty("HID Devices")]
        public IEnumerable<SerializedDeviceEndpoint> Devices { private set; get; }

        [JsonProperty("Console Log")]
        public IEnumerable<LogMessage> ConsoleLog { private set; get; }

        private static string GetAppVersion()
        {
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            return $"OpenTabletDriver v{version}";
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
