using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.ApplicationLifetimes;
using HidSharp;
using Newtonsoft.Json;
using OpenTabletDriver.Windows;
using TabletDriverPlugin;
using TabletDriverPlugin.Logging;

namespace OpenTabletDriver.Diagnostics
{
    public class DiagnosticDump
    {
        [JsonProperty("App Version")]
        public string AppVersion { private set; get; } = GetAppVersion();

        [JsonProperty("Operating System")]
        public OperatingSystem OperatingSystem { private set; get; } = Environment.OSVersion;

        [JsonProperty("Environment Variables")]
        public IDictionary EnvironmentVariables { private set; get; } = Environment.GetEnvironmentVariables();

        [JsonProperty("HID Devices")]
        public IEnumerable<HidDevice> Devices { private set; get; } = DeviceList.Local.GetHidDevices();

        [JsonProperty("Console Log")]
        public IEnumerable<LogMessage> ConsoleLog { private set; get; } = GetLogMessages();

        private static string GetAppVersion()
        {
            return "OpenTabletDriver v" + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        private static IEnumerable<LogMessage> GetLogMessages()
        {
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return (IEnumerable<LogMessage>)(desktop.MainWindow as MainWindow).ViewModel.Messages;
            else
                return null;
        }

        private static IEnumerable<string> GetLogDumpFormatted()
        {
            var lines = from message in GetLogMessages()
                select Log.GetStringFormat(message).Replace('\t', ' ');
            yield return JsonConvert.SerializeObject(lines, Formatting.Indented);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}