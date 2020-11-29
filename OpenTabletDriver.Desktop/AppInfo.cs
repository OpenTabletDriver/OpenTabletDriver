using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Native;

namespace OpenTabletDriver.Desktop
{
    public class AppInfo
    {
        public static AppInfo Current { set; get; } = new AppInfo();
        
        public static DesktopPluginManager PluginManager { set; get; } = new DesktopPluginManager(Current.PluginDirectory);

        private string configDirectory, appDataDirectory;

        public string ConfigurationDirectory
        {
            set => configDirectory = value;
            get => configDirectory ??= DefaultConfigurationDirectory;
        }

        public string AppDataDirectory
        {
            set => appDataDirectory = value;
            get => appDataDirectory ??= DefaultAppDataDirectory;
        }

        public string SettingsFile => Path.Join(AppDataDirectory, "settings.json");
        public string PluginDirectory => Path.Join(AppDataDirectory, "Plugins");

        private static string ProgramDirectory => AppContext.BaseDirectory;

        private static string DefaultConfigurationDirectory
        {
            get
            {
                var path = Path.Join(ProgramDirectory, "Configurations");
                var fallbackPath = Path.Join(Environment.CurrentDirectory, "Configurations");
                return Directory.Exists(path) ? path : fallbackPath;
            }
        }

        private static string DefaultAppDataDirectory
        {
            get
            {
                var path = Path.Join(ProgramDirectory, "userdata");
                var fallbackPath = SystemInfo.CurrentPlatform switch
                {
                    RuntimePlatform.Windows => Path.Join(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "OpenTabletDriver"),
                    RuntimePlatform.Linux => Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "OpenTabletDriver"),
                    RuntimePlatform.MacOS => Path.Join(Environment.GetEnvironmentVariable("HOME"), "Library", "Application Support", "OpenTabletDriver"),
                    _ => null
                };
                return Directory.Exists(path) ? path : fallbackPath;
            }
        }
    }
}
