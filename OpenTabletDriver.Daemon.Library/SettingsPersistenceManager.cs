using System.IO;
using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.Daemon
{
    public class SettingsPersistenceManager : ISettingsPersistenceManager
    {
        public Settings Settings { get; private set; } = new Settings();

        public Settings Load(FileInfo file)
        {
            file.Refresh();
            if (!file.Exists)
                return new Settings();

            Settings = Settings.Deserialize(file) ?? new Settings();
            return Settings;
        }

        public void Save(Settings settings, FileInfo file)
        {
            Settings = settings;
            settings.Serialize(file);
            file.Refresh();
        }
    }
}
