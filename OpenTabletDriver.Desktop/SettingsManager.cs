using System.IO;
using OpenTabletDriver.Desktop.Interop.AppInfo;

namespace OpenTabletDriver.Desktop
{
    public class SettingsManager : ISettingsManager
    {
        private readonly FileInfo _settingsFile;

        public SettingsManager(IAppInfo appInfo)
        {
            _settingsFile = new FileInfo(appInfo.SettingsFile);

            Settings = Settings.GetDefaults();
        }

        public Settings Settings { set; get; }

        public bool Load() => Load(_settingsFile);

        public void Save() => Save(_settingsFile);

        public bool Load(FileInfo file)
        {
            if (file.Exists)
                Settings = Settings.Deserialize(file)!;
            return file.Exists;
        }

        public void Save(FileInfo file)
        {
            Settings.Serialize(file);
            file.Refresh();
        }
    }
}
