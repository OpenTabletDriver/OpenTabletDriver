using System;
using System.IO;
using System.Reflection;
using OpenTabletDriver.Native;

namespace OpenTabletDriver
{
    public class AppInfo
    {
        public static readonly AppInfo Current = new AppInfo();

        private string _configDirectory, _appDataDirectory;

        public string ConfigurationDirectory
        {
            set => _configDirectory = value;
            // get => _configDirectory ?? _defaultConfigurationDirectory.Value;
            get
            {
                if (Directory.Exists(_configDirectory))
                {
                    return _configDirectory;
                }
                else if (Directory.Exists(_defaultConfigurationDirectory.Value))
                {
                    return _defaultConfigurationDirectory.Value;
                }
                else
                {
                    return Path.Join(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "Configurations"
                    );
                }
            }
        }

        public string AppDataDirectory
        {
            set => _appDataDirectory = value;
            get => _appDataDirectory ?? _defaultAppDataDirectory.Value;
        }

        public string ConfigFile => Path.Join(AppDataDirectory, "config.json");
        public string ProfileDirectory => Path.Join(AppDataDirectory, "Profiles");
        public string PluginDirectory => Path.Join(AppDataDirectory, "Plugins");

        private static readonly Lazy<string> _defaultConfigurationDirectory = new Lazy<string>(() =>
            Path.Join(Environment.CurrentDirectory, "Configurations"));

        private static readonly Lazy<string> _defaultAppDataDirectory = new Lazy<string>(() =>
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => Path.Join(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "OpenTabletDriver"),
                RuntimePlatform.Linux => Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "OpenTabletDriver"),
                RuntimePlatform.MacOS => Path.Join(Environment.GetEnvironmentVariable("HOME"), "Library", "Application Support", "OpenTabletDriver"),
                _ => null
            };
        });
    }
}