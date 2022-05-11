using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public class WindowsUpdater : GithubUpdater
    {
        public WindowsUpdater()
           : this(null,
               AppDomain.CurrentDomain.BaseDirectory,
               AppInfo.Current.AppDataDirectory,
               AppInfo.Current.BackupDirectory)
        {
        }

        public WindowsUpdater(Version? currentVersion, string binDirectory, string appDataDirectory, string rollBackDirectory)
            : base(currentVersion,
                binDirectory,
                appDataDirectory,
                rollBackDirectory)
        {
        }

        protected override string[] IncludeList { get; } = new[]
        {
            "OpenTabletDriver.UX.Wpf.exe",
            "OpenTabletDriver.Daemon.exe"
        };

        protected override async Task Download(GithubRelease ghRelease)
        {
            var release = ghRelease.Release;
            var asset = release.Assets.First(r => r.Name.Contains("win-x64"));

            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(asset.BrowserDownloadUrl))
            using (var zipStream = new ZipArchive(stream))
            {
                zipStream.ExtractToDirectory(DownloadDirectory);
            }
        }
    }
}
