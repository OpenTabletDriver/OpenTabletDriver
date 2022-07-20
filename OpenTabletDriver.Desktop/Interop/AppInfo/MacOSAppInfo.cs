namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class MacOSAppInfo : AppInfo
    {
        public MacOSAppInfo()
        {
            AppDataDirectory = GetPath("~/Library/Application Support/OpenTabletDriver");
            TemporaryDirectory = GetPath("$TMPDIR/OpenTabletDriver");
            CacheDirectory = GetPath("~/Library/Caches/OpenTabletDriver");
        }
    }
}
