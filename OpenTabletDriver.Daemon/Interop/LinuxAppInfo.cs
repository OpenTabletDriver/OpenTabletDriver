using static OpenTabletDriver.Daemon.Contracts.FileUtilities;

namespace OpenTabletDriver.Daemon.Interop
{
    public class LinuxAppInfo : Contracts.AppInfo
    {
        public LinuxAppInfo()
        {
            if (GetExistingPath(ConfigurationDirectory, "$XDG_DATA_HOME/OpenTabletDriver/Configurations", "~/.local/share/OpenTabletDriver/Configurations") is string config)
                ConfigurationDirectory = config;

            if (GetPath(AppDataDirectory, "$XDG_CONFIG_HOME/OpenTabletDriver", "~/.config/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath(TemporaryDirectory, "$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath(CacheDirectory, "$XDG_CACHE_HOME/OpenTabletDriver", "~/.cache/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
