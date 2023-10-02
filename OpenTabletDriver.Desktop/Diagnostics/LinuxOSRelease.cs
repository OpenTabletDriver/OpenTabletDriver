using System;
using System.IO;
using System.Collections.Generic;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class LinuxOSRelease
    {
        private static readonly string[] OS_RELEASE_PATHS = {
            "/etc/os-release",
            "/usr/lib/os-release"
        };

        private static readonly string[] ACCEPTED_KEYS = {
            "ID",
            "ID_LIKE",
            "PRETTY_NAME",
            "VARIANT",
            "VARIANT_ID",
            "VERSION",
            "VERSION_ID",
            "VERSION_CODENAME",
            "BUILD_ID",
            "SUPPORT_END"
        };

        private static string? _usedPath;
        public static string UsedPath => _usedPath ??= GetOSReleasePath();

        private static IDictionary<string, string>? _raw;
        public static IDictionary<string, string> Raw => _raw ??= Parse();

        private static IDictionary<string,string> Parse()
        {
            IDictionary<string, string> rv = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(UsedPath))
                return rv;

            using (StreamReader reader = File.OpenText(UsedPath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains('='))
                        continue;

                    // ignore comments
                    if (line.StartsWith('#'))
                        continue;

                    var parsed = ParseLine(line);

                    // check whitelist
                    if (Array.IndexOf(ACCEPTED_KEYS, parsed.Key) == -1)
                        continue;

                    if (rv.ContainsKey(parsed.Key)) // spec says that later lines override earlier lines
                        rv[parsed.Key] = parsed.Value;

                    rv.Add(parsed);
                }
            }

            return rv;
        }

        private static KeyValuePair<string, string> ParseLine(string line)
        {
            var split = line.Split('=');
            string key = split[0];
            string val = split[1].Trim('"'); // trim outer quotes
            return new KeyValuePair<string, string>(key, val);
        }

        private static string GetOSReleasePath()
        {
            foreach (var path in OS_RELEASE_PATHS)
            {
                if (!File.Exists(path))
                    continue;

                return path;
            }

            return string.Empty;
        }
    }
}
