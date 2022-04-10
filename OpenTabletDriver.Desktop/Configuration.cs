using System;
using System.IO;
using OpenTabletDriver.Plugin;
using Tommy;

namespace OpenTabletDriver.Desktop
{
    public static class Configuration
    {
        public static void Load(string path)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                GenerateAndApplyDefaults(path);
                return;
            }

            using (var reader = file.OpenText())
            {
                TomlTable table;
                try
                {
                    table = TOML.Parse(reader);
                    ApplyConfig(table);
                }
                catch
                {
                    GenerateAndApplyDefaults(path);
                    Log.Write("Configuration", "Failed to parse configuration file. Using defaults.", LogLevel.Warning);
                }
            }
        }

        private static void GenerateAndApplyDefaults(string path)
        {
            var logLevelStrings = string.Join(", ", Enum.GetNames<LogLevel>());
            var config = new TomlTable
            {
                ["loglevel"] = new TomlString
                {
                    Value = LogLevel.Info.ToString(),
                    Comment = $"The minimum level of log messages to output. Valid values are {logLevelStrings}"
                }
            };

            using (var writer = File.CreateText(path))
            {
                config.WriteTo(writer);
            }

            ApplyConfig(config);
        }

        private static void ApplyConfig(TomlTable config)
        {
            var logLevelString = config["loglevel"].AsString;
            if (logLevelString.HasValue && Enum.TryParse<LogLevel>(logLevelString.Value, out var logLevel))
            {
                Log.Verbosity = logLevel;
            }
        }
    }
}
