using System;
using System.IO;
using NativeLib;

namespace TabletDriverLib
{
    public static class AppInfo
    {
        public static DirectoryInfo ConfigurationDirectory => new DirectoryInfo(Path.Join(Environment.CurrentDirectory, "Configurations"));
        public static DirectoryInfo AppDataDirectory => _appDataDirectory.Value;
        public static FileInfo SettingsFile => new FileInfo(Path.Join(AppDataDirectory.FullName, "settings.json"));

        private static readonly Lazy<DirectoryInfo> _appDataDirectory = new Lazy<DirectoryInfo>(() => 
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