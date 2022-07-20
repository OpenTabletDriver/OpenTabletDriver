using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Desktop.Interop.AppInfo;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public class WindowsUpdater : GitHubUpdater
    {
        public WindowsUpdater(IAppInfo appInfo, IGitHubClient client)
           : base(AssemblyVersion, appInfo, client)
        {
        }

        protected override string[] IncludeList { get; } =
        {
            "OpenTabletDriver.UX.Wpf.exe",
            "OpenTabletDriver.Daemon.exe"
        };

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
