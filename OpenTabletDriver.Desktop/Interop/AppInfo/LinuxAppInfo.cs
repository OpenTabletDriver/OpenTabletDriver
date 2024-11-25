namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using System.IO;
    using static FileUtilities;

    public class LinuxAppInfo : AppInfo
    {
        public LinuxAppInfo()
        {
            if (GetExistingPath(ConfigurationDirectory, "$XDG_DATA_HOME/OpenTabletDriver/Configurations", "~/.local/share/OpenTabletDriver/Configurations") is string config)
                ConfigurationDirectory = config;

            if (GetExistingPathOrLast(AppDataDirectory, Path.Join(ProgramDirectory, "userdata"), "$XDG_CONFIG_HOME/OpenTabletDriver", "~/.config/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath(TemporaryDirectory, "$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath(CacheDirectory, "$XDG_CACHE_HOME/OpenTabletDriver", "~/.cache/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
