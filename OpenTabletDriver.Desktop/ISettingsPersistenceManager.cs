using System.IO;

namespace OpenTabletDriver.Desktop
{
    public interface ISettingsPersistenceManager
    {
        Settings? Load(FileInfo file);
        void Save(Settings settings, FileInfo file);
    }
}
