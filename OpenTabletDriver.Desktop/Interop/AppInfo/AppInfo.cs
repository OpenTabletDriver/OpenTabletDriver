using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    public class AppInfo : IAppInfo
    {
        private string? _appDataDirectory,
            _binaryDirectory,
            _configurationDirectory,
            _settingsFile,
            _pluginDirectory,
            _presetDirectory,
            _logDirectory,
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

        public string LogDirectory
        {
            set => _logDirectory = value;
            get => _logDirectory ?? GetDefaultLogDirectory();
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

        public static string ProgramDirectory
        {
            get
            {
                if (OperatingSystem.IsMacOS())
                {
                    var match = Regex.Match(AppContext.BaseDirectory, "^(.*)/[^/]+\\.app/Contents/MacOS/?$",
                        RegexOptions.IgnoreCase);
                    if (match.Success)
                        return match.Groups[1].Value;
                }

                return AppContext.BaseDirectory;
            }
        }

        public static IAppInfo GetPlatformAppInfo()
        {
            if(OperatingSystem.IsWindows())
                return new WindowsAppInfo();
            if(OperatingSystem.IsLinux())
                return new LinuxAppInfo();
            if (OperatingSystem.IsMacOS())
                return new MacOSAppInfo();

            throw new PlatformNotSupportedException("This platform is not supported by OpenTabletDriver.");
        }

        private static string GetDefaultAppDataDirectory() => EnvironmentVariable("OTD_APPDATA");
        private static string GetDefaultBinaryDirectory() => AppDomain.CurrentDomain.BaseDirectory;

        private string GetDefaultConfigurationDirectory() => FileUtilities.GetExistingPathOrLast(
            EnvironmentVariable("OTD_CONFIGURATIONS"),
            Path.Join(AppDataDirectory, "Configurations"),
            Path.Join(ProgramDirectory, "Configurations"),
            Path.Join(System.Environment.CurrentDirectory, "Configurations")
        );

        private string GetDefaultSettingsFile() => Path.Join(AppDataDirectory, "settings.json");
        private string GetDefaultPluginDirectory() => Path.Join(AppDataDirectory, "Plugins");
        private string GetDefaultPresetDirectory() => Path.Join(AppDataDirectory, "Presets");
        private string GetDefaultLogDirectory() => Path.Join(AppDataDirectory, "Logs");
        private string GetDefaultTemporaryDirectory() => Path.Join(AppDataDirectory, "Temp");
        private string GetDefaultCacheDirectory() => Path.Join(AppDataDirectory, "Cache");
        private string GetDefaultBackupDirectory() => Path.Join(AppDataDirectory, "Backup");
        private string GetDefaultTrashDirectory() => Path.Join(AppDataDirectory, "Trash");

        private static string EnvironmentVariable(string envVar) => FileUtilities.InjectEnvironmentVariables(System.Environment.GetEnvironmentVariable(envVar));
    }
}
