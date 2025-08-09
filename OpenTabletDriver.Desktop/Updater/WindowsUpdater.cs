using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public sealed class WindowsUpdater : GitHubUpdater
    {
        public WindowsUpdater(AppInfo appInfo, IGitHubClient client)
           : base(AssemblyVersion, appInfo, client)
        {
        }

        protected override void RunElevated(Update update, string binaryRollback)
        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = Path.Join(BinaryDirectory, "OpenTabletDriver.Daemon.exe"),
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                WorkingDirectory = BinaryDirectory,
            };

            processInfo.ArgumentList.Add("update");

            if (!string.IsNullOrEmpty(AppInfo.Current.AppDataDirectory))
            {
                processInfo.ArgumentList.Add("--appdata");
                processInfo.ArgumentList.Add(AppInfo.Current.AppDataDirectory);
            }

            processInfo.ArgumentList.Add("--sources");
            foreach (var path in update.Paths)
                processInfo.ArgumentList.Add(path);

            processInfo.ArgumentList.Add("--destination");
            processInfo.ArgumentList.Add(BinaryDirectory);

            var process = Process.Start(processInfo);
            if (process is null)
                throw new UpdateInstallFailedException("Failed to start update process");

            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new UpdateInstallFailedException($"Update failed with exit code {process.ExitCode}");
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
                ImmutableArray.Create(Directory.GetFileSystemEntries(downloadPath)),
                BinaryDirectory
            );
        }
    }
}
