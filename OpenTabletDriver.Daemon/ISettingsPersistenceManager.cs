using System.IO;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon
{
    public interface ISettingsPersistenceManager
    {
        public Settings Settings { get; }
        Settings Load(FileInfo file);
        void Save(Settings settings, FileInfo file);
    }
}
