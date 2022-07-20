using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTabletDriver.Desktop.Interop.AppInfo;

namespace OpenTabletDriver.Desktop
{
    public class PresetManager : IPresetManager
    {
        public PresetManager(IAppInfo appInfo)
        {
            _presetDirectory = new DirectoryInfo(appInfo.PresetDirectory);
        }

        private readonly DirectoryInfo _presetDirectory;

        private const string FILE_EXTENSION = ".json";

        private List<Preset> Presets { get; } = new List<Preset>();

        public IReadOnlyCollection<Preset> GetPresets() => Presets;

        public Preset FindPreset(string presetName)
        {
            return Presets.Find(preset => preset.Name == presetName);
        }

        private void Load()
        {
            foreach (var file in _presetDirectory.EnumerateFiles("*.json"))
            {
                var settings = Settings.Deserialize(file);
                if (settings != null)
                {
                    Presets.Add(new Preset(file.Name.Replace(file.Extension, string.Empty), settings));
                    Log.Write("Settings", $"Loaded preset '{file.Name}'", LogLevel.Info);
                }
                else
                {
                    Log.Write("Settings", $"Invalid settings file '{file.Name}' attempted to load into presets", LogLevel.Warning);
                }
            }
        }

        public void Refresh()
        {
            Presets.Clear();
            Load();
            Log.Write("Settings", $"Presets have been refreshed. Loaded {Presets.Count} presets.", LogLevel.Info);
        }

        public void Save(string name, Settings settings)
        {
            if (!_presetDirectory.Exists)
                _presetDirectory.Create();

            var files = _presetDirectory.EnumerateFiles("*" + FILE_EXTENSION);
            var oldFile = files.FirstOrDefault(f => f.Name.Replace(FILE_EXTENSION, string.Empty) == name);

            oldFile?.Delete();
            var path = Path.Join(_presetDirectory.FullName, name + FILE_EXTENSION);
            Serialization.Serialize(File.Create(path), settings);

            Refresh();
        }
    }
}
