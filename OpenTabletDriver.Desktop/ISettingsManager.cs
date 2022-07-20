using System.IO;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public interface ISettingsManager
    {
        Settings Settings { set; get; }

        bool Load();
        void Save();

        bool Load(FileInfo file);
        void Save(FileInfo file);
    }
}
