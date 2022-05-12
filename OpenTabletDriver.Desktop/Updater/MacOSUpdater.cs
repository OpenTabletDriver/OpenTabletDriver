using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

#pragma warning disable 618
#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public class MacOSUpdater : GithubUpdater
    {
        public MacOSUpdater()
            : this(null,
                AppDomain.CurrentDomain.BaseDirectory,
                AppInfo.Current.AppDataDirectory,
                AppInfo.Current.BackupDirectory)
        {
        }

        public MacOSUpdater(Version? currentVersion, string binDirectory, string appDataDirectory, string rollBackDirectory)
            : base(currentVersion,
                binDirectory,
                appDataDirectory,
                rollBackDirectory)
        {
        }

        protected override void PostInstall()
        {
            var subPath = Path.Join(BinaryDirectory, "OpenTabletDriver.app", "Contents", "MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.UX.MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.Daemon");
        }

        protected override async Task Download(GithubRelease ghRelease)
        {
            var release = ghRelease.Release;
            var asset = release.Assets.First(r => r.Name.Contains("osx-x64"));

            // Download and extract tar gzip
            using (var httpClient = new HttpClient())
            using (var httpStream = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl))
            using (var decompressionStream = new GZipStream(httpStream, CompressionMode.Decompress))
            using (var tar = TarArchive.CreateInputTarArchive(decompressionStream))
            {
                tar.ExtractContents(DownloadDirectory);
            }
        }
    }
}
