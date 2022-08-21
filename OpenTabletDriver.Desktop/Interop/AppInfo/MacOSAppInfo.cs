namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class MacOSAppInfo : AppInfo
    {
        public MacOSAppInfo()
        {
            if (GetPath(AppDataDirectory, "~/Library/Application Support/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath(TemporaryDirectory, "$TMPDIR/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath(CacheDirectory, "~/Library/Caches/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
