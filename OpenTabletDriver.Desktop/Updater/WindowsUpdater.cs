using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public class WindowsUpdater : Updater
    {
        public WindowsUpdater()
           : this(AssemblyVersion,
               AppDomain.CurrentDomain.BaseDirectory,
               AppInfo.Current.AppDataDirectory,
               AppInfo.Current.BackupDirectory)
        {
        }

        public WindowsUpdater(Version currentVersion, string binDirectory, string appDataDirectory, string rollBackDirectory)
            : base(currentVersion,
                binDirectory,
                appDataDirectory,
                rollBackDirectory)
        {
        }

        protected override async Task Download(Release release)
        {
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
