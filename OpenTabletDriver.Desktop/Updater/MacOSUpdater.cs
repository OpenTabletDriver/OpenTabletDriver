using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Octokit;
#pragma warning disable 618

namespace OpenTabletDriver.Desktop.Updater
{
    public class MacOSUpdater : Updater
    {
        public override async Task<string> Download(Release release)
        {
            var binaryDir = Path.Join(AppInfo.Current.TemporaryDirectory, release.TagName);
            var asset = GetAsset(release);

            // Download and extract tar gzip
            using (var httpClient = new HttpClient())
            using (var httpStream = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl))
            using (var decompressionStream = new GZipStream(httpStream, CompressionMode.Decompress))
            using (var tar = TarArchive.CreateInputTarArchive(decompressionStream))
            {
                tar.ExtractContents(binaryDir);
            }

            return binaryDir;
        }

        public override ReleaseAsset GetAsset(Release release)
        {
            return release.Assets.FirstOrDefault(r => r.Name.Contains("osx-x64"));
        }

        public override async Task Install(Release release, string targetDir)
        {
            var binaryDir = await Download(release);
            var oldDir = Path.Join(AppInfo.Current.TemporaryDirectory, CurrentVersion + "-old");

            if (Directory.Exists(targetDir))
                Directory.Move(targetDir, oldDir);

            var subPath = Path.Join(binaryDir, "OpenTabletDriver.app", "Contents", "MacOS");
            Directory.Move(subPath, targetDir);
        }
    }
}