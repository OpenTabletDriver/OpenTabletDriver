using System;
using System.IO;
using NativeLib;

namespace TabletDriverLib
{
    public class AppInfo
    {
        public static readonly AppInfo Current = new AppInfo();

        private string _configDirectory, _appDataDirectory;
        
        public string ConfigurationDirectory
        {
            set => _configDirectory = value;
            get => _configDirectory ?? _defaultConfigurationDirectory.Value;
        }

        public string AppDataDirectory
        {
            set => _appDataDirectory = value;
            get => _appDataDirectory ?? _defaultAppDataDirectory.Value;
        }

        public string SettingsFile => Path.Join(AppDataDirectory, "settings.json");
        public string PluginDirectory => Path.Join(AppDataDirectory, "Plugins");

        private static readonly Lazy<string> _defaultConfigurationDirectory = new Lazy<string>(() => 
            Path.Join(Environment.CurrentDirectory, "Configurations"));

        private static readonly Lazy<string> _defaultAppDataDirectory = new Lazy<string>(() => 
        {
            if (PlatformInfo.IsWindows)
            {
                var appdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                return Path.Join(appdata, "OpenTabletDriver");
            }
            else if (PlatformInfo.IsLinux)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return Path.Join(home, ".config", "OpenTabletDriver");
            }
            else if (PlatformInfo.IsOSX)
            {
                var macHome = Environment.GetEnvironmentVariable("HOME");
                return Path.Join(macHome, "Library", "Application Support", "OpenTabletDriver");
            }
            else
            {
                return null;
            }
        });
    }
}