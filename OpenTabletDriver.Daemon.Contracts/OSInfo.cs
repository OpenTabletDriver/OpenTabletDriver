using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Daemon
{
    public class OSInfo
    {
        public string Name { get; }
        public string Version { get; }
        public IDictionary<string, string>? Attributes { get; }

        public OSInfo(string name, string version, IDictionary<string, string>? attributes)
        {
            Name = name;
            Version = version;
            Attributes = attributes;
        }

        public static OSInfo GetOSInfo()
        {
            return SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => ExtractFromOS(withServicePack: true),
                SystemPlatform.Linux => GetLinuxOSInfo(),
                SystemPlatform.MacOS => ExtractFromOS(withServicePack: false),
                _ => throw new NotImplementedException()
            };
        }

        private static OSInfo GetLinuxOSInfo()
        {
            var osReleasePaths = new[] {
                "/etc/os-release",
                "/usr/lib/os-release"
            };

            var acceptedKeys = new HashSet<string>() {
                "NAME",
                "ID",
                "ID_LIKE",
                "PRETTY_NAME",
                "VARIANT",
                "VARIANT_ID",
                "VERSION",
                "VERSION_ID",
                "VERSION_CODENAME",
                "BUILD_ID",
                "SUPPORT_END",
            };

            // search for valid path
            var osReleasePath = osReleasePaths.First(x => File.Exists(x));
            if (osReleasePath == null)
            {
                // fallback
                return ExtractFromOS(false);
            }

            string name = "Linux";
            string version = "Unknown";
            var attributes = new Dictionary<string, string>();

            ParseOSRelease(osReleasePath, (key, value) =>
            {
                if (!acceptedKeys.Contains(key))
                    return;

                switch (key)
                {
                    case "NAME":
                        name = value;
                        break;
                    case "VERSION":
                        version = value;
                        break;
                    default:
                        attributes[key] = value;
                        break;
                }
            });

            // add linux version
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "uname",
                    Arguments = "-r",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            attributes.Add("KERNEL_VERSION", output.Trim());

            return new OSInfo(name, version, attributes);
        }

        private static OSInfo ExtractFromOS(bool withServicePack)
        {
            var operatingSystem = Environment.OSVersion;
            var name = operatingSystem.Platform.ToString();
            var version = operatingSystem.Version.ToString();
            var attributes = withServicePack ?
                new Dictionary<string, string>()
                {
                    ["ServicePack"] = operatingSystem.ServicePack
                }
                : null;

            return new OSInfo(name, version, attributes);
        }

        private static void ParseOSRelease(string osReleasePath, Action<string, string> action)
        {
            using (var reader = File.OpenText(osReleasePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('=', StringSplitOptions.TrimEntries);
                    if (parts.Length != 2)
                        continue;

                    var key = parts[0];
                    var value = parts[1].Trim('"');

                    action(key, value);
                }
            }
        }
    }
}
