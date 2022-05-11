using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class Updater<TInfo> : IUpdater where TInfo : UpdateInfo
    {
        /// <summary>
        /// <para>0 allows update install and check.</para>
        /// <para>1 disallows update install and check.</para>
        /// <para>2 means update was installed.</para>
        /// </summary>
        private int updateSentinel = 0;
        private TInfo? updateInfo;

        protected Version CurrentVersion { get; }
        protected string BinaryDirectory { get; }
        protected string AppDataDirectory { get; }
        protected string RollbackDirectory { get; }
        protected string DownloadDirectory { get; } = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        protected virtual string[]? IncludeList { get; } = null;

        public string? VersionedRollbackDirectory { private set; get; }

        protected Updater(Version? currentVersion, string binaryDir, string appDataDir, string rollbackDir)
        {
            CurrentVersion = currentVersion ?? typeof(IUpdater).Assembly.GetName().Version!;
            BinaryDirectory = binaryDir;
            RollbackDirectory = rollbackDir;
            AppDataDirectory = appDataDir;

            if (!Directory.Exists(RollbackDirectory))
                Directory.CreateDirectory(RollbackDirectory);
            if (!Directory.Exists(DownloadDirectory))
                Directory.CreateDirectory(DownloadDirectory);
        }

        protected abstract Task<TInfo> GetUpdate();
        protected abstract Task Download(TInfo release);

        public Task<bool> CheckForUpdates() => CheckForUpdates(true);

        private async Task<bool> CheckForUpdates(bool forced)
        {
            if (updateSentinel == 2)
                return false;

            try
            {
                if (forced || updateInfo == null)
                    updateInfo = await GetUpdate();

                return updateInfo!.Version > CurrentVersion;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return false;
            }
        }

        public async Task<UpdateInfo?> GetInfo()
        {
            if (updateSentinel == 2)
                return null;

            return await GetUpdate();
        }

        public async Task InstallUpdate()
        {
            // Skip if update is already installed, or in the process of installing
            if (Interlocked.CompareExchange(ref updateSentinel, 1, 0) == 0)
            {
                if (await CheckForUpdates(false) && updateInfo != null)
                {
                    try
                    {
                        await Install(updateInfo);
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

        protected void PerformBackup()
        {
            string timestamp = DateTime.UtcNow.ToString("-yyyy-MM-dd_hh-mm-ss");
            VersionedRollbackDirectory = Path.Join(RollbackDirectory, CurrentVersion + timestamp);

            InclusiveFileOp(BinaryDirectory, VersionedRollbackDirectory, "bin",
                static (source, target) => Move(source, target));
            ExclusiveFileOp(AppDataDirectory, RollbackDirectory, "appdata", VersionedRollbackDirectory,
                static (source, target) => Copy(source, target));
        }

        private async Task Install(TInfo updateInfo)
        {
            await Download(updateInfo);
            PerformBackup();

            Move(DownloadDirectory, BinaryDirectory);
            PostInstall();
        }

        protected virtual void PostInstall() { }

        // Avoid moving/copying the rollback directory if under source directory
        private static void ExclusiveFileOp(string source, string backupDir, string target, string versionBackupDir, Action<string, string> fileOp)
        {
            var backupTarget = Path.Join(versionBackupDir, target);

            var excludeList = new[]
            {
                backupDir,
                versionBackupDir,
                Path.Join(source, "userdata")
            };

            var childEntries = Directory.EnumerateFileSystemEntries(source)
                .Except(excludeList);

            PerformFileOperations(fileOp, backupTarget, childEntries);
        }

        private void InclusiveFileOp(string source, string backupDir, string target, Action<string, string> fileOp)
        {
            var backupTarget = Path.Join(backupDir, target);

            var entries = Directory.EnumerateFileSystemEntries(source);

            if (IncludeList != null)
                entries = entries.Intersect(IncludeList.Select(t => Path.Join(source, t)));

            PerformFileOperations(fileOp, backupTarget, entries);
        }

        private static void PerformFileOperations(Action<string, string> fileOp, string targetParentDir, IEnumerable<string> entries)
        {
            if (Directory.Exists(targetParentDir))
                Directory.Delete(targetParentDir, true);
            Directory.CreateDirectory(targetParentDir);

            foreach (var entry in entries)
            {
                var fileName = Path.GetFileName(entry);
                var path = Path.Join(targetParentDir, fileName);
                fileOp(entry, path);
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
