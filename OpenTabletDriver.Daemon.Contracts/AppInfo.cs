using System;
using System.IO;
using System.Text.RegularExpressions;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class AppInfo
    {
        private string? _appDataDirectory,
            _binaryDirectory,
            _configurationDirectory,
            _settingsFile,
            _pluginDirectory,
            _presetDirectory,
            _temporaryDirectory,
            _cacheDirectory,
            _backupDirectory,
            _trashDirectory;

        public string AppDataDirectory
        {
            set => _appDataDirectory = value;
            get => _appDataDirectory ?? GetDefaultAppDataDirectory();
        }

        public string BinaryDirectory
        {
            set => _binaryDirectory = value;
            get => _binaryDirectory ?? GetDefaultBinaryDirectory();
        }

        public string ConfigurationDirectory
        {
            set => _configurationDirectory = value;
            get => _configurationDirectory ?? GetDefaultConfigurationDirectory();
        }

        public string SettingsFile
        {
            set => _settingsFile = value;
            get => _settingsFile ?? GetDefaultSettingsFile();
        }

        public string PluginDirectory
        {
            set => _pluginDirectory = value;
            get => _pluginDirectory ?? GetDefaultPluginDirectory();
        }

        public string PresetDirectory
        {
            set => _presetDirectory = value;
            get => _presetDirectory ?? GetDefaultPresetDirectory();
        }

        public string TemporaryDirectory
        {
            set => _temporaryDirectory = value;
            get => _temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set => _cacheDirectory = value;
            get => _cacheDirectory ?? GetDefaultCacheDirectory();
        }

        public string BackupDirectory
        {
            set => _backupDirectory = value;
            get => _backupDirectory ?? GetDefaultBackupDirectory();
        }

        public string TrashDirectory
        {
            set => _trashDirectory = value;
            get => _trashDirectory ?? GetDefaultTrashDirectory();
        }

        public static string ProgramDirectory => SystemInterop.CurrentPlatform switch
        {
            SystemPlatform.MacOS => Regex.Match(AppContext.BaseDirectory, "^(.*)/[^/]+\\.app/Contents/MacOS/?$", RegexOptions.IgnoreCase) switch
            {
                { Success: true } match => match.Groups[1].ToString(),
                _ => AppContext.BaseDirectory
            },
            _ => AppContext.BaseDirectory
        };

        private static string GetDefaultAppDataDirectory() => EnvironmentVariable("OTD_APPDATA");
        private static string GetDefaultBinaryDirectory() => AppDomain.CurrentDomain.BaseDirectory;

        private string GetDefaultConfigurationDirectory() => FileUtilities.GetExistingPathOrLast(
            EnvironmentVariable("OTD_CONFIGURATIONS"),
            Path.Join(AppDataDirectory, "Configurations"),
            Path.Join(ProgramDirectory, "Configurations"),
            Path.Join(Environment.CurrentDirectory, "Configurations")
        );

        private string GetDefaultSettingsFile() => Path.Join(AppDataDirectory, "settings.json");
        private string GetDefaultPluginDirectory() => Path.Join(AppDataDirectory, "Plugins");
        private string GetDefaultPresetDirectory() => Path.Join(AppDataDirectory, "Presets");
        private string GetDefaultTemporaryDirectory() => Path.Join(AppDataDirectory, "Temp");
        private string GetDefaultCacheDirectory() => Path.Join(AppDataDirectory, "Cache");
        private string GetDefaultBackupDirectory() => Path.Join(AppDataDirectory, "Backup");
        private string GetDefaultTrashDirectory() => Path.Join(AppDataDirectory, "Trash");

        private static string EnvironmentVariable(string envVar) => FileUtilities.InjectEnvironmentVariables(Environment.GetEnvironmentVariable(envVar));
    }
}
