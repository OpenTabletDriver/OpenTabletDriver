using System.IO;
using System.Collections.Generic;
using OpenTabletDriver.Plugin;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop
{
    public class PresetManager
    {
        public PresetManager()
        {
            PresetDirectory = new DirectoryInfo(AppInfo.Current.PresetDirectory);
        }

        public DirectoryInfo PresetDirectory { get; }

        private List<Preset> Presets { get; } = new List<Preset>();

        public IReadOnlyCollection<Preset> GetPresets() => Presets;

        public Preset FindPreset(string presetName)
        {
            return Presets.Find(preset => preset.Name == presetName);
        }

        private void Load()
        {
            foreach (var preset in PresetDirectory.EnumerateFiles("*.json"))
            {
                var settings = Settings.Deserialize(preset);
                if (settings != null)
                {
                    Presets.Add(new Preset(preset.Name.Replace(preset.Extension, string.Empty), settings));
                    Log.Write("Settings", $"Loaded preset '{preset.Name}'", LogLevel.Info);
                }
                else
                {
                    Log.Write("Settings", $"Invalid settings file '{preset.Name}' attempted to load into presets", LogLevel.Warning);
                }
            }
        }

        public void Refresh()
        {
            Presets.Clear();
            Load();
            Log.Write("Settings", $"Presets have been refreshed. Loaded {Presets.Count} presets.", LogLevel.Info);
        }

        public class Preset
        {
            public Preset(string name, Settings settings)
            {
                Name = name;
                Settings = settings;
            }

            public string Name { get; }
            private Settings Settings;

            public Settings GetSettings()
            {
                return JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(Settings));
            }
        }
    }
}
