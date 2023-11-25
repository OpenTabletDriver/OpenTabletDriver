using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using OpenTabletDriver.Daemon.Contracts.Persistence;

#nullable enable

namespace OpenTabletDriver.Console
{
    public static class Extensions
    {
        public static string Format(this PluginSettings? settings)
        {
            return settings == null ? "None" : settings.ToString();
        }

        public static string Format(this IEnumerable<PluginSettings?> settings)
        {
            return string.Join(", ", settings.FormatEnumerable());
        }

        public static string ComputeFileHash(this SHA256 sha256, string path)
        {
            var data = File.ReadAllBytes(path);
            var hash = sha256.ComputeHash(data);
            return string.Join(null, hash.Select(b => b.ToString("X")));
        }

        private static IEnumerable<string> FormatEnumerable(this IEnumerable<PluginSettings?> settings)
        {
            foreach (var setting in settings)
                yield return setting.Format();
        }
    }
}
