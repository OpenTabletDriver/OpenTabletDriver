using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Plugin;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class Updater : IUpdater
    {
        /// <summary>
        /// <para>0 allows update install and check.</para>
        /// <para>1 disallows update install and check.</para>
        /// <para>2 means update was installed and check will return false.</para>
        /// </summary>
        private int updateSentinel = 0;
        private readonly GitHubClient github = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));
        private Release? latestRelease;

        protected Version CurrentVersion { get; }
        protected static readonly Version AssemblyVersion = typeof(IUpdater).Assembly.GetName().Version!;
        protected string BinaryDirectory { get; }
        protected string AppDataDirectory { get; }
        protected string RollbackDirectory { get; }
        protected string DownloadDirectory { get; } = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());

        public string? VersionedRollbackDirectory { private set; get; }

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

        public Task<bool> CheckForUpdates() => CheckForUpdates(true);

        public async Task<bool> CheckForUpdates(bool forced)
        {
            if (updateSentinel == 2)
                return false;

            try
            {
                if (forced || latestRelease == null)
                    latestRelease = await github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");

                var latestVersion = new Version(latestRelease!.TagName[1..]); // remove `v` from `vW.X.Y.Z
                return latestVersion > CurrentVersion;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return false;
            }
        }

        public async Task<Release?> GetRelease()
        {
            if (latestRelease == null)
            {
                await CheckForUpdates();
            }

            return latestRelease;
        }

        public async Task InstallUpdate()
        {
            // Skip if update is already installed, or in the process of installing
            if (Interlocked.CompareExchange(ref updateSentinel, 1, 0) == 0)
            {
                if (await CheckForUpdates(false))
                {
                    try
                    {
                        await Install(latestRelease!);
                        updateSentinel = 2;
                        return;
                    }
                    catch
                    {
                        updateSentinel = 0;
                        throw;
                    }
                }
                updateSentinel = 0;
            }
        }

        protected void SetupRollback()
        {
            string timestamp = DateTime.UtcNow.ToString("-yyyy-MM-dd_hh-mm-ss");
            VersionedRollbackDirectory = Path.Join(RollbackDirectory, CurrentVersion + timestamp);

            ExclusiveFileOp(BinaryDirectory, RollbackDirectory, VersionedRollbackDirectory, "bin",
                static (source, target) => Move(source, target));
            ExclusiveFileOp(AppDataDirectory, RollbackDirectory, VersionedRollbackDirectory, "appdata",
                static (source, target) => Copy(source, target));
        }

        protected virtual async Task Install(Release release)
        {
            await Download(release);
            SetupRollback();

            Move(DownloadDirectory, BinaryDirectory);
        }

        protected abstract Task Download(Release release);

        // Avoid moving/copying the rollback directory if under source directory
        private static void ExclusiveFileOp(string source, string rollbackDir, string versionRollbackDir, string target,
            Action<string, string> fileOp)
        {
            var rollbackTarget = Path.Join(versionRollbackDir, target);

            var childEntries = Directory
                .EnumerateFileSystemEntries(source)
                .Except(new[] { rollbackDir, versionRollbackDir, Path.Join(source, "userdata") });

            if (Directory.Exists(rollbackTarget))
                Directory.Delete(rollbackTarget, true);
            Directory.CreateDirectory(rollbackTarget);

            foreach (var childEntry in childEntries)
            {
                fileOp(childEntry, Path.Join(rollbackTarget, Path.GetFileName(childEntry)));
            }
        }

        protected static void Move(string source, string target)
        {
            if (File.Exists(source))
            {
                var sourceFile = new FileInfo(source);
                sourceFile.MoveTo(target);
                return;
            }

            var sourceDir = new DirectoryInfo(source);
            var targetDir = new DirectoryInfo(target);
            if (targetDir.Exists)
            {
                foreach (var childEntry in sourceDir.EnumerateFileSystemInfos())
                {
                    if (childEntry is FileInfo file)
                    {
                        file.MoveTo(Path.Join(target, file.Name));
                    }
                    else if (childEntry is DirectoryInfo directory)
                    {
                        directory.MoveTo(Path.Join(target, directory.Name));
                    }
                }
            }
            else
            {
                Directory.Move(source, target);
            }
        }

        protected static void Copy(string source, string target)
        {
            if (File.Exists(source))
            {
                var sourceFile = new FileInfo(source);
                sourceFile.CopyTo(target);
                return;
            }

            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);

            var sourceDir = new DirectoryInfo(source);
            foreach (var fileInfo in sourceDir.EnumerateFileSystemInfos())
            {
                if (fileInfo is FileInfo file)
                {
                    file.CopyTo(Path.Join(target, file.Name));
                }
                else if (fileInfo is DirectoryInfo directory)
                {
                    Copy(directory.FullName, Path.Join(target, directory.Name));
                }
            }
        }
    }
}
