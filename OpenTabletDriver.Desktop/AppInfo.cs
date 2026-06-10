using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    using static FileUtilities;

    public class AppInfo
    {
        private string? configurationDirectory,
            settingsFile,
            pluginDirectory,
            presetDirectory,
            logDirectory,
            temporaryDirectory,
            cacheDirectory,
            backupDirectory,
            trashDirectory;

        public AppInfo()
        {
            // on Linux, verify presence of necessary environment variables (as '~' expands to $HOME environment variable)
            if (SystemInterop.CurrentPlatform == PluginPlatform.Linux && IsEnvVarUnset("HOME") && IsEnvVarUnset("XDG_DATA_HOME"))
            {
                Log.Write(nameof(AppInfo),
                    "Unable to look up environment variable 'HOME' or 'XDG_DATA_HOME'. '~/.local/share/OpenTabletDriver' paths will not detect (such as configuration overrides).",
                    LogLevel.Warning);
            }
        }

        private static AppInfo? current;

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
                    ConfigurationDirectory = GetExistingPath(Path.Join(UnixXdgPath.DataHome, "OpenTabletDriver/Configurations")),
                    AppDataDirectory = GetExistingPathOrLast(Path.Join(ProgramDirectory, "userdata"), Path.Join(UnixXdgPath.ConfigHome, "OpenTabletDriver")),
                    TemporaryDirectory = GetPath(Path.Join(UnixXdgPath.RuntimeDir, "OpenTabletDriver")),
                    CacheDirectory = GetPath(Path.Join(UnixXdgPath.CacheHome, "OpenTabletDriver")),
                },
                PluginPlatform.MacOS => new AppInfo()
                {
                    AppDataDirectory = GetExistingPathOrLast(Path.Join(ProgramDirectory, "userdata"), "~/Library/Application Support/OpenTabletDriver"),
                    TemporaryDirectory = GetPath("$TMPDIR/OpenTabletDriver"),
                    CacheDirectory = GetPath("~/Library/Caches/OpenTabletDriver"),
                },
                _ => throw new InvalidOperationException($"Unsupported platform {SystemInterop.CurrentPlatform}"),
            };
        }

        public static DesktopPluginManager PluginManager { set; get; } = new DesktopPluginManager();

        public static PresetManager PresetManager { set; get; } = new PresetManager();

        public required string AppDataDirectory { set; get; }

        [AllowNull]
        public string ConfigurationDirectory
        {
            set => this.configurationDirectory = value;
            get => this.configurationDirectory ?? GetDefaultConfigurationDirectory();
        }

        [AllowNull]
        public string SettingsFile
        {
            set => this.settingsFile = value;
            get => this.settingsFile ?? GetDefaultSettingsFile();
        }

        [AllowNull]
        public string PluginDirectory
        {
            set => this.pluginDirectory = value;
            get => this.pluginDirectory ?? GetDefaultPluginDirectory();
        }

        [AllowNull]
        public string PresetDirectory
        {
            set => this.presetDirectory = value;
            get => this.presetDirectory ?? GetDefaultPresetDirectory();
        }

        [AllowNull]
        public string LogDirectory
        {
            set => this.logDirectory = value;
            get => this.logDirectory ?? GetDefaultLogDirectory();
        }

        [AllowNull]
        public string TemporaryDirectory
        {
            set => this.temporaryDirectory = value;
            get => this.temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        [AllowNull]
        public string CacheDirectory
        {
            set => this.cacheDirectory = value;
            get => this.cacheDirectory ?? GetDefaultCacheDirectory();
        }

        [AllowNull]
        public string BackupDirectory
        {
            set => this.backupDirectory = value;
            get => this.backupDirectory ?? GetDefaultBackupDirectory();
        }

        [AllowNull]
        public string TrashDirectory
        {
            set => this.trashDirectory = value;
            get => this.trashDirectory ?? GetDefaultTrashDirectory();
        }

        public static string ProgramDirectory => AppContext.BaseDirectory;

        private string GetDefaultConfigurationDirectory() => GetExistingPathOrLast(
            Path.Join(AppDataDirectory, "Configurations"),
            Path.Join(ProgramDirectory, "Configurations"),
            Path.Join(Environment.CurrentDirectory, "Configurations")
        );

        private string GetDefaultSettingsFile() => Path.Join(AppDataDirectory, "settings.json");
        private string GetDefaultPluginDirectory() => Path.Join(AppDataDirectory, "Plugins");
        private string GetDefaultPresetDirectory() => Path.Join(AppDataDirectory, "Presets");
        private string GetDefaultLogDirectory() => Path.Join(AppDataDirectory, "Logs");
        private string GetDefaultTemporaryDirectory() => Path.Join(AppDataDirectory, "Temp");
        private string GetDefaultCacheDirectory() => Path.Join(AppDataDirectory, "Cache");
        private string GetDefaultBackupDirectory() => Path.Join(AppDataDirectory, "Backup");
        private string GetDefaultTrashDirectory() => Path.Join(AppDataDirectory, "Trash");

        private static bool IsEnvVarUnset(string envVar) =>
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar));
    }
}
