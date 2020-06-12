using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HidSharp;
using Newtonsoft.Json;
using TabletDriverPlugin.Logging;

namespace TabletDriverLib.Diagnostics
{
    public class DiagnosticInfo
    {
        public DiagnosticInfo(IEnumerable<LogMessage> log)
        {
            ConsoleLog = log;
        }

        [JsonProperty("App Version")]
        public string AppVersion { private set; get; } = GetAppVersion();

        [JsonProperty("Operating System")]
        public OperatingSystem OperatingSystem { private set; get; } = Environment.OSVersion;

        [JsonProperty("Environment Variables")]
        public IDictionary EnvironmentVariables { private set; get; } = Environment.GetEnvironmentVariables();

        [JsonProperty("HID Devices")]
        public IEnumerable<HidDevice> Devices { private set; get; } = DeviceList.Local.GetHidDevices();

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