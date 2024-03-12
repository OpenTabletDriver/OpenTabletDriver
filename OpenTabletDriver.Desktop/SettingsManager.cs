using System;
using System.IO;
using OpenTabletDriver.Desktop.Interop.AppInfo;

namespace OpenTabletDriver.Desktop
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly FileInfo _settingsFile;

        public SettingsManager(IServiceProvider serviceProvider, IAppInfo appInfo)
        {
            _serviceProvider = serviceProvider;
            _settingsFile = new FileInfo(appInfo.SettingsFile);

            Settings = Settings.GetDefaults();
        }

        private static readonly Version SupportedRevision = Version.Parse("0.7.0.0");

        public Settings Settings { set; get; }

        public bool Load() => Load(_settingsFile);

        public void Save() => Save(_settingsFile);

        public bool Load(FileInfo file)
        {
            file.Refresh();
            if (!file.Exists)
                return false;

            if (Settings.TryDeserialize(file, out var newSettings))
                Settings = newSettings;

            return newSettings != null;
        }

        public void Save(FileInfo file)
        {
            Settings.Serialize(file);
            file.Refresh();
        }

        // private void Migrate(IAppInfo appInfo)
        // {
        //     var file = new FileInfo(appInfo.SettingsFile);

        //     if (Migrate(file) is Settings settings)
        //     {
        //         // Back up existing settings file for safety
        //         var backupDir = appInfo.BackupDirectory;
        //         if (!Directory.Exists(backupDir))
        //             Directory.CreateDirectory(backupDir);

        //         var timestamp = DateTime.UtcNow.ToString(".yyyy-MM-dd_hh-mm-ss");
        //         var backupPath = Path.Join(backupDir, file.Name + timestamp + ".old");
        //         file.CopyTo(backupPath, true);

        //         Serialization.Serialize(file, settings);
        //     }
        // }

        // private Settings? Migrate(FileInfo file)
        // {
        //     file.Refresh();
        //     if (!file.Exists)
        //         return null;

        //     if (Settings.Deserialize(file)?.Revision < SupportedRevision)
        //     {
        //         var settingsV6 = Serialization.Deserialize<Migration.LegacySettings.V6.Settings>(file);
        //         return settingsV6?.Migrate(_serviceProvider);
        //     }

        //     return null;
        // }
    }
}
