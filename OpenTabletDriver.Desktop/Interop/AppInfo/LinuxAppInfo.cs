namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class LinuxAppInfo : AppInfo
    {
        public LinuxAppInfo()
        {
            ConfigurationDirectory = GetExistingPath("$XDG_DATA_HOME/OpenTabletDriver/Configurations", "~/.local/share/OpenTabletDriver/Configurations");
            AppDataDirectory = GetPath("$XDG_CONFIG_HOME/OpenTabletDriver", "~/.config/OpenTabletDriver");
            TemporaryDirectory = GetPath("$XDG_RUNTIME_DIR/OpenTabletDriver", "$TEMP/OpenTabletDriver");
            CacheDirectory = GetPath("$XDG_CACHE_HOME/OpenTabletDriver", "~/.cache/OpenTabletDriver");
        }
    }
}
