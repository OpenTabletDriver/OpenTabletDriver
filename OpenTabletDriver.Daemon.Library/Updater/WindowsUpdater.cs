using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Updater
{
    public sealed class WindowsUpdater : GitHubUpdater
    {
        public WindowsUpdater(AppInfo appInfo, IGitHubClient client)
           : base(AssemblyVersion, appInfo, client)
        {
        }

        protected override async Task<Update> Download(Release release, Version version)
        {
            var downloadPath = GetDownloadPath();
            var asset = release.Assets.First(r => r.Name.Contains("win-x64"));

            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(asset.BrowserDownloadUrl))
            using (var zipStream = new ZipArchive(stream))
            {
                zipStream.ExtractToDirectory(downloadPath);
            }

            return new Update(
                version,
                ImmutableArray.Create(Directory.GetFileSystemEntries(downloadPath))
            );
        }
    }
}
