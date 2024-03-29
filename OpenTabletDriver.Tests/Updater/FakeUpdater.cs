using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Updater;
using OpenTabletDriver.Tests.Fakes;
using Xunit;
using OTDUpdater = OpenTabletDriver.Daemon.Updater.Updater;

namespace OpenTabletDriver.Tests.Updater
{
    public class FakeUpdater : OTDUpdater
    {
        private readonly string _downloadPath = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        private Update? _update;

        public FakeUpdater(UpdaterEnvironment env) : base(env.Version, env.BinaryDir, env.AppDataDir, env.RollBackDir)
        {
            env.HookToUpdater(this);
            Environment = env;
            if (!Directory.Exists(_downloadPath))
                Directory.CreateDirectory(_downloadPath);
        }

        public UpdaterEnvironment Environment { get; }
        public List<FakeFileSystemEntry> UpdateFiles { get; set; } = new();

        public async Task CreateUpdateAsync(Version version)
        {
            await FakeFileSystemEntry.SetupFilesAsync(_downloadPath, UpdateFiles);

            _update = new Update(
                version,
                ImmutableArray.Create(Directory.GetFileSystemEntries(_downloadPath))
            );
        }

        public async Task SimulateFailedInstallAsync<TException>(Update update) where TException : Exception, new()
        {
            UpdateInstalling += Throw;
            await Assert.ThrowsAsync<TException>(() => Install(update));
            UpdateInstalling -= Throw;

            static void Throw(Update update) => throw new TException();
        }

        protected override Task<UpdateInfo?> CheckForUpdatesCore()
        {
            if (_update is null)
                return Task.FromResult<UpdateInfo?>(null);

            var updateInfo = new UpdateInfo(() => Task.FromResult(_update))
            {
                Version = _update.Version
            };

            return Task.FromResult<UpdateInfo?>(updateInfo);
        }
    }
}
