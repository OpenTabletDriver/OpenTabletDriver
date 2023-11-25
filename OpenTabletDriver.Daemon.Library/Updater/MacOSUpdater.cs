using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Octokit;
using OpenTabletDriver.Daemon.Contracts;

#pragma warning disable 618
namespace OpenTabletDriver.Daemon.Updater
{
    public class MacOSUpdater : GitHubUpdater
    {
        public MacOSUpdater(AppInfo appInfo, IGitHubClient client)
            : base(AssemblyVersion, appInfo, client)
        {
            UpdateInstalled += _ => PostInstall();
        }

        private void PostInstall()
        {
            // Mark the binaries executable, SharpZipLib doesn't do this.
            var subPath = Path.Join(BinaryDirectory, "OpenTabletDriver.app", "Contents", "MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.UX.MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.Daemon");
        }

        protected override async Task<Update> Download(Release release, Version version)
        {
            var downloadPath = GetDownloadPath();
            var asset = release.Assets.First(r => r.Name.Contains("osx-x64"));

            // Download and extract tar gzip
            using (var httpClient = new HttpClient())
            await using (var httpStream = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl))
            await using (var decompressionStream = new GZipStream(httpStream, CompressionMode.Decompress))
            using (var tar = TarArchive.CreateInputTarArchive(decompressionStream))
            {
                tar.ExtractContents(downloadPath);
            }

            return new Update(
                version,
                ImmutableArray.Create(Directory.GetFileSystemEntries(downloadPath))
            );
        }
    }
}
