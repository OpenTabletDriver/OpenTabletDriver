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

        public static AppInfo Current
        {
            set;
            get => field ??= SystemInterop.CurrentPlatform switch
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
                    CacheDirectory = GetPath("~/Library/Caches/OpenTabletDriver")
                },
                _ => throw new InvalidOperationException($"Unsupported platform {SystemInterop.CurrentPlatform}"),
            };
        }

        public static DesktopPluginManager PluginManager { set; get; } = new DesktopPluginManager();

        public static PresetManager PresetManager { set; get; } = new PresetManager();

        public required string AppDataDirectory { set; get; }

        public string ConfigurationDirectory
        {
            set;
            get => field ?? GetDefaultConfigurationDirectory();
        }

        public string SettingsFile
        {
            set;
            get => field ?? GetDefaultSettingsFile();
        }

        public string PluginDirectory
        {
            set;
            get => field ?? GetDefaultPluginDirectory();
        }

        public string PresetDirectory
        {
            set;
            get => field ?? GetDefaultPresetDirectory();
        }

        public string LogDirectory
        {
            set;
            get => field ?? GetDefaultLogDirectory();
        }

        public string TemporaryDirectory
        {
            set;
            get => field ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set;
            get => field ?? GetDefaultCacheDirectory();
        }

        public string BackupDirectory
        {
            set;
            get => field ?? GetDefaultBackupDirectory();
        }

        public string TrashDirectory
        {
            set;
            get => field ?? GetDefaultTrashDirectory();
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

        private static bool IsEnvVarUnset([DisallowNull] string envVar) =>
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar));
    }
}
