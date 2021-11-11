using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace OpenTabletDriver.Desktop.Updater
{
    public class WindowsUpdater : Updater
    {
        public override async Task<string> Download(Release release)
        {
            var binaryDir = Path.Join(AppInfo.Current.TemporaryDirectory, release.TagName);
            var tempZipPath = Path.Join(AppInfo.Current.TemporaryDirectory, Path.GetRandomFileName() + ".zip");

            var asset = GetAsset(release);

            // Download file to temporary location
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(asset.BrowserDownloadUrl))
            using (var tempZipFile = File.Create(tempZipPath))
            {
                await stream.CopyToAsync(tempZipFile);
            }

            // Extract
            ZipFile.ExtractToDirectory(tempZipPath, binaryDir);
            return binaryDir;
        }

        public override ReleaseAsset GetAsset(Release release)
        {
            return release.Assets.FirstOrDefault(r => r.Name.Contains("win-x64"));
        }
    }
}