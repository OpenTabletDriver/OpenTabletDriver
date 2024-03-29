using System;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Updater
{
    public sealed class UpdateInfo
    {
        private readonly Func<Task<Update>> _updateFactory;
        private Update? _update;

        public UpdateInfo(Func<Task<Update>> updateFactory)
        {
            _updateFactory = updateFactory;
        }

        public Version Version { get; init; } = new Version();

        public async Task<Update> GetUpdate()
        {
            _update ??= await _updateFactory();
            return _update;
        }

        public UpdateInfoDto ToSerializedUpdateInfo() => new(Version);
    }
}
