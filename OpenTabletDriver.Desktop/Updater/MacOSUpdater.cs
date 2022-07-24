using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Octokit;
using OpenTabletDriver.Desktop.Interop.AppInfo;

#pragma warning disable 618
namespace OpenTabletDriver.Desktop.Updater
{
    public class MacOSUpdater : GitHubUpdater
    {
        public MacOSUpdater(IAppInfo appInfo, IGitHubClient client)
            : base(AssemblyVersion, appInfo, client)
        {
        }

        protected override async Task Install(Release release)
        {
            await Download(release);
            PerformBackup();

            // Mark the binaries executable, SharpZipLib doesn't do this.
            var subPath = Path.Join(DownloadDirectory, "OpenTabletDriver.app", "Contents", "MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.UX.MacOS");
            Process.Start("chmod", $"+x {subPath}/OpenTabletDriver.Daemon");
            Move(subPath, BinaryDirectory);
        }

        protected override async Task Download(Release release)
        {
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
