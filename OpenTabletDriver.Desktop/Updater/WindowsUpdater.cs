using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

        protected override void InstallUpdateFallback(Update update, Exception ex)
        {
            var targetProcesses = new string[] {
                "OpenTabletDriver.UX.Wpf.exe",
                "OpenTabletDriver.Daemon.exe",
                "OpenTabletDriver.Console.exe",
            };

            var batchCommands = new List<string>()
            {
                "@echo off"
            };

            // Kill all processes that are being updated
            batchCommands.AddRange(targetProcesses
                .Select(p => $"taskkill /F /IM \"{p}\" > nul"));

            // Install the update
            batchCommands.AddRange(update.Paths
                .Select(p => $"move /Y \"{p}\" \"{BinaryDirectory}\""));

            // Start the updated process
            batchCommands.Add(
                $$"""
                set "executable="
                for /R "{{BinaryDirectory}}" %%F in (OpenTabletDriver.UI*) do (
                    if not defined executable (
                        set "executable=%%F"
                    )
                )

                if defined executable (
                    start "" "%executable%"
                ) else (
                    start "" "{{Path.Join(BinaryDirectory, "OpenTabletDriver.UX.Wpf.exe")}}"
                )
                """
            );

            var batchCommand = string.Join("\n", batchCommands);
            var updateScript = Path.Join(Path.GetTempPath(), Path.GetRandomFileName() + ".bat");
            File.WriteAllText(updateScript, batchCommand);

            var processInfo = new ProcessStartInfo()
            {
                FileName = updateScript,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = BinaryDirectory,
            };

            Process.Start(processInfo);
            Thread.Sleep(Timeout.Infinite);
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
