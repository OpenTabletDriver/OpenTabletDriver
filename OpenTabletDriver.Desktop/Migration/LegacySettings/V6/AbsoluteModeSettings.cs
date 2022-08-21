using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class AbsoluteModeSettings
    {
        [JsonProperty(nameof(Display))]
        public AreaSettings? Display { set; get; }

        [JsonProperty(nameof(Tablet))]
        public AreaSettings? Tablet { set; get; }

        [JsonProperty(nameof(EnableClipping))]
        public bool EnableClipping { set; get; }

        [JsonProperty(nameof(EnableAreaLimiting))]
        public bool EnableAreaLimiting { set; get; }

        [JsonProperty(nameof(LockAspectRatio))]
        public bool LockAspectRatio { set; get; }
    }
}
