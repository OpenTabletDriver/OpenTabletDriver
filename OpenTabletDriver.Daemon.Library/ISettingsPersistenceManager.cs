using System.IO;
using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.Daemon.Library
{
    public interface ISettingsPersistenceManager
    {
        public Settings Settings { get; }
        Settings Load(FileInfo file);
        void Save(Settings settings, FileInfo file);
    }
}
