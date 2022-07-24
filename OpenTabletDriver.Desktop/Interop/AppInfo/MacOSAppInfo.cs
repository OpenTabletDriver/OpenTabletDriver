namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    using static FileUtilities;

    public class MacOSAppInfo : AppInfo
    {
        public MacOSAppInfo()
        {
            if (GetPath("~/Library/Application Support/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath("$TMPDIR/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath("~/Library/Caches/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
