using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    public class AbsoluteModeSettings : NotifyPropertyChanged
    {
        [JsonProperty("Display")]
        public AreaSettings? Display { set; get; }

        [JsonProperty("Tablet")]
        public AreaSettings? Tablet { set; get; }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping { set; get; }

        [JsonProperty("EnableAreaLimiting")]
        public bool EnableAreaLimiting { set; get; }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio { set; get; }
    }
}
