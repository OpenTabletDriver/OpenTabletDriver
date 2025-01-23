using System;
using System.IO;
using System.Text.RegularExpressions;
using static OpenTabletDriver.Daemon.Contracts.FileUtilities;

namespace OpenTabletDriver.Daemon.Library.Interop
{
    public class MacOSAppInfo : Contracts.AppInfo
    {
        public MacOSAppInfo()
        {
            var match = Regex.Match(AppContext.BaseDirectory, "^(.*)/[^/]+\\.app/Contents/MacOS/?$", RegexOptions.IgnoreCase);

            if (match.Success)
                BinaryDirectory = match.Groups[1].ToString();

            if (GetExistingPathOrLast(AppDataDirectory, Path.Join(ProgramDirectory, "userdata"), "~/Library/Application Support/OpenTabletDriver") is string appdata)
                AppDataDirectory = appdata;

            if (GetPath(TemporaryDirectory, "$TMPDIR/OpenTabletDriver") is string temp)
                TemporaryDirectory = temp;

            if (GetPath(CacheDirectory, "~/Library/Caches/OpenTabletDriver") is string cache)
                CacheDirectory = cache;
        }
    }
}
