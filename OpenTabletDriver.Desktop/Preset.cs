using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop
{
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
