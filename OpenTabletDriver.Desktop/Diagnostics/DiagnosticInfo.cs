using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
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

        [JsonProperty("Operating System")]
        public OperatingSystem OperatingSystem { private set; get; } = Environment.OSVersion;

        [JsonProperty("Environment Variables")]
        public IDictionary<string, string> EnvironmentVariables { private set; get; } = new EnvironmentDictionary();

        [JsonProperty("HID Devices")]
        public IEnumerable<SerializedDeviceEndpoint> Devices { private set; get; }

        [JsonProperty("Console Log")]
        public IEnumerable<LogMessage> ConsoleLog { private set; get; }

        private static string GetAppVersion()
        {
            return "OpenTabletDriver v" + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
