using System;
using System.IO;
using System.Linq;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    using static FileUtilities;

    public class AppInfo
    {
        private string configurationDirectory,
            settingsFile,
            pluginDirectory,
            presetDirectory,
            temporaryDirectory,
            cacheDirectory,
            backupDirectory,
            trashDirectory;

        private static AppInfo current;
        public static AppInfo Current
        {
            set => current = value;
            get => current ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => new AppInfo
                {
                    AppDataDirectory = GetExistingPathOrLast(Path.Join(ProgramDirectory, "userdata"), "$LOCALAPPDATA\\OpenTabletDriver")
                },
                PluginPlatform.Linux => new AppInfo
                {
                    ConfigurationDirectory = GetExistingPath("$XDG_DATA_HOME/OpenTabletDriver/Configurations", "~/.local/share/OpenTabletDriver/Configurations"),
                    AppDataDirectory = GetPath("$XDG_CONFIG_HOME/OpenTabletDriver", "~/.config/OpenTabletDriver"),
                    TemporaryDirectory = GetPath("$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver"),
                    CacheDirectory = GetPath("$XDG_CACHE_HOME/OpenTabletDriver", "~/.cache/OpenTabletDriver"),
                },
                PluginPlatform.MacOS => new AppInfo()
                {
                    AppDataDirectory = GetPath("~/Library/Application Support/OpenTabletDriver"),
                    TemporaryDirectory = GetPath("$TMPDIR/OpenTabletDriver"),
                    CacheDirectory = GetPath("~/Library/Caches/OpenTabletDriver")
                },
                _ => null
            };
        }

        public static DesktopPluginManager PluginManager { get; } = new DesktopPluginManager();

        public static PresetManager PresetManager { get; } = new PresetManager();

        public string AppDataDirectory { set; get; }

        public string ConfigurationDirectory
        {
            set => this.configurationDirectory = value;
            get => this.configurationDirectory ?? GetDefaultConfigurationDirectory();
        }

        public string SettingsFile
        {
            set => this.settingsFile = value;
            get => this.settingsFile ?? GetDefaultSettingsFile();
        }

        public string PluginDirectory
        {
            set => this.pluginDirectory = value;
            get => this.pluginDirectory ?? GetDefaultPluginDirectory();
        }

        public string PresetDirectory
        {
            set => this.presetDirectory = value;
            get => this.presetDirectory ?? GetDefaultPresetDirectory();
        }

        public string TemporaryDirectory
        {
            set => this.temporaryDirectory = value;
            get => this.temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set => this.cacheDirectory = value;
            get => this.cacheDirectory ?? GetDefaultCacheDirectory();
        }

        public string BackupDirectory
        {
            set => this.backupDirectory = value;
            get => this.backupDirectory ?? GetDefaultBackupDirectory();
        }

        public string TrashDirectory
        {
            set => this.trashDirectory = value;
            get => this.trashDirectory ?? GetDefaultTrashDirectory();
        }

        public static string ProgramDirectory => AppContext.BaseDirectory;

        private static string GetDirectory(params string[] directories)
        {
            foreach (var dir in directories.Select(InjectEnvironmentVariables))
                if (Path.IsPathRooted(dir))
                    return dir;

            return null;
        }

        private static string GetDirectoryIfExists(params string[] directories)
        {
            foreach (var dir in directories.Select(InjectEnvironmentVariables))
                if (Directory.Exists(dir))
                    return dir;

            return InjectEnvironmentVariables(directories.Last());
        }

        private string GetDefaultConfigurationDirectory() => GetExistingPathOrLast(
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
    }
}
