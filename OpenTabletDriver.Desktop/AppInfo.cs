using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class AppInfo
    {
        private string configurationDirectory, settingsFile, pluginDirectory, temporaryDirectory, cacheDirectory, trashDirectory;
        private static AppInfo _current;

        public static AppInfo Current
        {
            set => _current = value;
            get => _current ??= SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => new AppInfo
                {
                    AppDataDirectory = GetDirectoryIfExists(Path.Join(ProgramDirectory, "userdata"), "$LOCALAPPDATA\\OpenTabletDriver")
                },
                PluginPlatform.Linux => new AppInfo
                {
                    AppDataDirectory = GetDirectory("$XDG_CONFIG_HOME/OpenTabletDriver", "$HOME/.config/OpenTabletDriver"),
                    TemporaryDirectory = GetDirectory("$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver"),
                    CacheDirectory = GetDirectory("$XDG_CACHE_HOME/OpenTabletDriver", "$HOME/.cache/OpenTabletDriver")
                },
                PluginPlatform.MacOS => new AppInfo()
                {
                    AppDataDirectory = GetDirectory("$HOME/Library/Application Support/OpenTabletDriver"),
                    TemporaryDirectory = GetDirectory("$TMPDIR/OpenTabletDriver"),
                    CacheDirectory = GetDirectory("$HOME/Library/Caches/OpenTabletDriver")
                },
                _ => null
            };
        }

        public static DesktopPluginManager PluginManager { get; } = new DesktopPluginManager();

        public virtual string AppDataDirectory { set; get; }

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

        public string TemporaryDirectory
        {
            set => this.temporaryDirectory = value;
            get => this.temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set => this.cacheDirectory = value;
            get => this.temporaryDirectory ?? GetDefaultCacheDirectory();
        }

        public string TrashDirectory
        {
            set => this.trashDirectory = value;
            get => this.trashDirectory ?? GetDefaultTrashDirectory();
        }

        protected static string ProgramDirectory => AppContext.BaseDirectory;

        private static string GetDirectory(params string[] directories)
        {
            foreach (var dir in directories.Select(d => InjectVariablesIntoPath(d)))
                if (Path.IsPathRooted(dir))
                    return dir;

            return null;
        }

        private static string GetDirectoryIfExists(params string[] directories)
        {
            foreach (var dir in directories.Select(d => InjectVariablesIntoPath(d)))
                if (Directory.Exists(dir))
                    return dir;

            return directories.Last();
        }

        private string GetDefaultConfigurationDirectory() => GetDirectoryIfExists(
            Path.Join(ProgramDirectory, "Configurations"),
            Path.Join(Environment.CurrentDirectory, "Configurations")
        );

        private string GetDefaultSettingsFile() => Path.Join(AppDataDirectory, "settings.json");
        private string GetDefaultPluginDirectory() => Path.Join(AppDataDirectory, "Plugins");
        private string GetDefaultTemporaryDirectory() => Path.Join(AppDataDirectory, "Temp");
        private string GetDefaultCacheDirectory() => Path.Join(AppDataDirectory, "Cache");
        private string GetDefaultTrashDirectory() => Path.Join(AppDataDirectory, "Trash");

        private static string InjectVariablesIntoPath(string str)
        {
            StringBuilder sb = new StringBuilder(str);
            sb.Replace("~", Environment.GetEnvironmentVariable("HOME"));

            foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
            {
                string key = envVar.Key as string;
                string value = envVar.Value as string;
                sb.Replace($"${key}", value); // $KEY
                sb.Replace($"${{{key}}}", value); // ${KEY}
            }

            return sb.ToString();
        }
    }
}
