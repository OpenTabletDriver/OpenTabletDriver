using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public abstract class AppInfo
    {
        public static AppInfo Current { set; get; } = DesktopInterop.CurrentPlatform switch
        {
            PluginPlatform.Windows => new WindowsAppInfo(),
            PluginPlatform.Linux => new LinuxAppInfo(),
            PluginPlatform.MacOS => new MacOSAppInfo(),
            _ => null
        };

        public static DesktopPluginManager PluginManager { get; } = new DesktopPluginManager();

        private string appDataDir, configDir;

        public virtual string AppDataDirectory
        {
            set => this.appDataDir = value;
            get
            {
                if (!string.IsNullOrWhiteSpace(this.appDataDir))
                    return this.appDataDir;

                // Allows for userdata folder to be created
                var userData = Path.Join(ProgramDirectory, "userdata");
                return this.appDataDir = Directory.Exists(userData) ? userData : GetDefaultAppDataDirectory();
            }
        }

        public virtual string ConfigurationDirectory
        {
            set => this.configDir = value;
            get => this.configDir ??= GetDefaultConfigurationDirectory();
        }

        public virtual string SettingsFile => Path.Join(AppDataDirectory, "settings.json");
        public virtual string PluginDirectory => Path.Join(AppDataDirectory, "Plugins");
        public virtual string TemporaryDirectory => Path.Join(AppDataDirectory, "Temp");
        public virtual string CacheDirectory => Path.Join(AppDataDirectory, "Cache");
        public virtual string TrashDirectory => Path.Join(AppDataDirectory, "Trash");

        protected static string ProgramDirectory => AppContext.BaseDirectory;

        protected abstract string GetDefaultAppDataDirectory();

        protected virtual string GetDefaultConfigurationDirectory()
        {
            var path = Path.Join(ProgramDirectory, "Configurations");
            var fallbackPath = Path.Join(Environment.CurrentDirectory, "Configurations");
            return Directory.Exists(path) ? path : fallbackPath;
        }

        private static string GetDirectory(params string[] directories)
        {
            foreach (var dir in directories.Select(d => InjectVariables(d)))
                if (Path.IsPathRooted(dir))
                    return dir;

            return null;
        }

        private static string InjectVariables(string str)
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

        private class WindowsAppInfo : AppInfo
        {
            protected override string GetDefaultAppDataDirectory() => GetDirectory("$LOCALAPPDATA\\OpenTabletDriver");
        }

        private class LinuxAppInfo : AppInfo
        {
            protected override string GetDefaultAppDataDirectory() => GetDirectory("$XDG_CONFIG_HOME/OpenTabletDriver", "$HOME/.config/OpenTabletDriver");
            public override string TemporaryDirectory => GetDirectory("$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver", base.TemporaryDirectory);
            public override string CacheDirectory => GetDirectory("$XDG_CACHE_HOME/OpenTabletDriver", "$HOME/.cache/OpenTabletDriver", base.CacheDirectory);
        }

        private class MacOSAppInfo : AppInfo
        {
            protected override string GetDefaultAppDataDirectory() => GetDirectory("$HOME/Library/Application Support/OpenTabletDriver");
            public override string TemporaryDirectory => GetDirectory("$TMPDIR/OpenTabletDriver", base.TemporaryDirectory);
            public override string CacheDirectory => GetDirectory("$HOME/Library/Caches/OpenTabletDriver", base.CacheDirectory);
        }
    }
}
