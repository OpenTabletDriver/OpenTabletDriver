using System;
using System.IO;
using NativeLib;

namespace TabletDriverLib
{
    public static class AppInfo
    {
        private static DirectoryInfo _configDirectory, _appDataDirectory;
        
        public static DirectoryInfo ConfigurationDirectory
        {
            set => _configDirectory = value;
            get => _configDirectory ?? new DirectoryInfo(Path.Join(Environment.CurrentDirectory, "Configurations"));
        }

        public static DirectoryInfo AppDataDirectory
        {
            set => _appDataDirectory = value;
            get => _appDataDirectory ?? _defaultAppDataDirectory.Value;
        }

        public static FileInfo SettingsFile => new FileInfo(Path.Join(AppDataDirectory.FullName, "settings.json"));
        public static DirectoryInfo PluginDirectory => new DirectoryInfo(Path.Join(AppDataDirectory.FullName, "Plugins"));

        private static readonly Lazy<DirectoryInfo> _defaultAppDataDirectory = new Lazy<DirectoryInfo>(() => 
        {
            if (PlatformInfo.IsWindows)
            {
                var appdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                return new DirectoryInfo(Path.Join(appdata, "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsLinux)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(home, ".config", "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsOSX)
            {
                var macHome = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(macHome, "Library", "Application Support", "OpenTabletDriver"));
            }
            else
            {
                return null;
            }
        });
    }
}