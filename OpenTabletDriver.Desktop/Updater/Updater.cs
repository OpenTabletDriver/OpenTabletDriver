using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Octokit;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class Updater : IUpdater
    {
        private int updateSentinel = 1;
        private readonly GitHubClient github = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));
        private Release? latestRelease;

        protected readonly Version CurrentVersion;
        protected static readonly Version AssemblyVersion = new Version(typeof(IUpdater).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
        protected string BinaryDirectory;
        protected string AppDataDirectory;
        protected string RollbackDirectory;
        protected string DownloadDirectory = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

        public Task<bool> HasUpdate => CheckForUpdates(true);

        protected Updater(Version? currentVersion, string binaryDir, string appDataDir, string rollbackDir)
        {
            CurrentVersion = currentVersion ?? AssemblyVersion;
            BinaryDirectory = binaryDir;
            RollbackDirectory = rollbackDir;
            AppDataDirectory = appDataDir;

            if (!Directory.Exists(RollbackDirectory))
                Directory.CreateDirectory(RollbackDirectory);
            if (!Directory.Exists(DownloadDirectory))
                Directory.CreateDirectory(DownloadDirectory);
        }

        public async Task InstallUpdate()
        {
            // Skip if update is already installed, or in the process of installing
            if (Interlocked.CompareExchange(ref updateSentinel, 0, 1) == 1)
            {
                if (await CheckForUpdates(false))
                {
                    SetupRollback();
                    await Install(latestRelease!);
                }
            }
        }

        protected abstract Task Install(Release release);

        private async Task<bool> CheckForUpdates(bool forced)
        {
            if (forced || latestRelease == null)
                latestRelease = await github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver"); ;

            var latestVersion = new Version(latestRelease.TagName[1..]); // remove `v` from `vW.X.Y.Z
            return latestVersion > CurrentVersion;
        }

        private void SetupRollback()
        {
            var versionRollbackDir = Path.Join(RollbackDirectory, CurrentVersion + "-old");

            ExclusiveMove(BinaryDirectory, versionRollbackDir, "bin");
            // ExclusiveMove(AppDataDirectory, versionRollbackDir, "data");
        }

        // Avoid moving the rollback directory if under source directory
        private static void ExclusiveMove(string source, string versionRollbackDir, string target)
        {
            var rollbackTarget = Path.Join(versionRollbackDir, target);

            var childEntries = Directory
                .EnumerateFileSystemEntries(source)
                .Except(new[] { versionRollbackDir });

            if (!Directory.Exists(rollbackTarget))
                Directory.CreateDirectory(rollbackTarget);

            foreach (var childEntry in childEntries)
            {
                Move(childEntry, rollbackTarget);
            }
        }

        protected static void Move(string source, string targetDir)
        {
            if (File.Exists(source))
            {
                File.Move(source, Path.Join(targetDir, Path.GetFileName(source)));
            }
            else if (Directory.Exists(source))
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                foreach (var childEntry in Directory.EnumerateFileSystemEntries(source))
                {
                    if (File.Exists(childEntry))
                    {
                        File.Move(childEntry, Path.Join(targetDir, Path.GetFileName(childEntry)));
                    }
                    else if (Directory.Exists(childEntry))
                    {
                        Directory.Move(childEntry, Path.Join(targetDir, Path.GetFileName(childEntry)));
                    }
                }
            }
            else
            {
                throw new ArgumentException("Provided source path is not a file or directory", nameof(source));
            }
        }
    }
}