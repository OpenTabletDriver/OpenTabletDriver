namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class LinuxAppInfo : AppInfo
    {
        public LinuxAppInfo()
        {
            if (GetExistingPath("$XDG_DATA_HOME/OpenTabletDriver/Configurations", "~/.local/share/OpenTabletDriver/Configurations") is string config)
                ConfigurationDirectory = config;

            if (GetPath("$XDG_CONFIG_HOME/OpenTabletDriver", "~/.config/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath("$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath("$XDG_CACHE_HOME/OpenTabletDriver", "~/.cache/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
