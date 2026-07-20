using System;
using System.Diagnostics;
using System.IO;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public enum XdgPaths
    {
        XdgDataHome,
        XdgConfigHome,
        XdgCacheHome,
        XdgRuntimeDir,
    }

    internal record struct XdgPathRecord(string EnvVar, string FallbackPath);

    /// <summary>
    /// Class for arbitrating XDG paths.
    /// String properties in this class will automatically pick the most fitting path for you.
    /// </summary>
    public static class UnixXdgPath
    {
        public static string ConfigHome => Lookup(XdgPaths.XdgConfigHome);
        public static string DataHome => Lookup(XdgPaths.XdgDataHome);
        public static string CacheHome => Lookup(XdgPaths.XdgCacheHome);
        public static string RuntimeDir => Lookup(XdgPaths.XdgRuntimeDir);

        private static XdgPathRecord GetPathRecord(XdgPaths path) => path switch
        {
            XdgPaths.XdgConfigHome => new XdgPathRecord("XDG_CONFIG_HOME", "~/.config"),
            XdgPaths.XdgDataHome => new XdgPathRecord("XDG_DATA_HOME", "~/.local/share"),
            XdgPaths.XdgCacheHome => new XdgPathRecord("XDG_CACHE_HOME", "~/.cache"),
            XdgPaths.XdgRuntimeDir => new XdgPathRecord("XDG_RUNTIME_DIR", "$TEMP"),
            _ => throw new ArgumentOutOfRangeException(nameof(path), path, $"Path '{path}' is not supported yet"),
        };

        private static string Lookup(XdgPaths path)
        {
            var pathRecord = GetPathRecord(path);

            string? pathFromEnvVar = Environment.GetEnvironmentVariable(pathRecord.EnvVar);
            bool found = !string.IsNullOrEmpty(pathFromEnvVar);

            string rv = FileUtilities.InjectEnvironmentVariables(found ? pathFromEnvVar! : pathRecord.FallbackPath);

            Log.Debug(nameof(UnixXdgPath),
                found
                    ? $"{pathRecord.EnvVar} found: '{rv}'"
                    : $"{pathRecord.EnvVar} not found, falling back to '{rv}'");

            if (!Directory.Exists(rv))
                Log.Write(nameof(UnixXdgPath), $"Returning non-existent directory '{rv}'");

            Debug.Assert(!string.IsNullOrEmpty(rv));
            return rv;
        }
    }
}
