using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenTabletDriver.Tests.Fakes;
using OTDUpdater = OpenTabletDriver.Daemon.Updater.Updater;

namespace OpenTabletDriver.Tests.Updater
{
    public sealed class UpdaterEnvironment : IDisposable
    {
        public Version Version { get; }
        public string BinaryDir { get; } = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        public string AppDataDir { get; } = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        public string RollBackDir { get; }

        public string? VersionedRollbackDirectory { get; private set; }

        public UpdaterEnvironment(Version version)
        {
            Version = version;
            RollBackDir = Path.Join(AppDataDir, "Temp");

            InitializeDirectory(BinaryDir);
            InitializeDirectory(AppDataDir);
            InitializeDirectory(RollBackDir);
        }

        public List<FakeFileSystemEntry> BinaryFiles { get; } = new();
        public List<FakeFileSystemEntry> AppDataFiles { get; } = new();

        internal void HookToUpdater(OTDUpdater updater)
        {
            updater.RollbackCreated += (path) => VersionedRollbackDirectory = path;
        }

        public async Task SetupFilesAsync()
        {
            await FakeFileSystemEntry.SetupFilesAsync(BinaryDir, BinaryFiles);
            await FakeFileSystemEntry.SetupFilesAsync(AppDataDir, AppDataFiles);
        }

        private static void InitializeDirectory(string directory)
        {
            CleanDirectory(directory);
            Directory.CreateDirectory(directory);
        }

        private static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        public void Dispose()
        {
            CleanDirectory(BinaryDir);
            CleanDirectory(AppDataDir);
            CleanDirectory(RollBackDir);
        }
    }
}
