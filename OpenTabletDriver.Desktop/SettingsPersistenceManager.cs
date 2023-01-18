using System;
using System.IO;

namespace OpenTabletDriver.Desktop
{
    public class SettingsPersistenceManager : ISettingsPersistenceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly Version _supportedRevision = Version.Parse("0.7.0.0");

        public SettingsPersistenceManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Settings? Load(FileInfo file)
        {
            file.Refresh();
            if (!file.Exists)
                return null;

            return Migrate(file) ?? Settings.Deserialize(file);
        }

        public void Save(Settings settings, FileInfo file)
        {
            settings.Serialize(file);
            file.Refresh();
        }

        private Settings? Migrate(FileInfo file)
        {
            file.Refresh();
            if (!file.Exists)
                return null;

            if (Settings.Deserialize(file)?.Revision < _supportedRevision)
            {
                var settingsV6 = Serialization.Deserialize<Migration.LegacySettings.V6.Settings>(file);
                return settingsV6?.Migrate(_serviceProvider);
            }

            return null;
        }
    }
}
