using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    public class Profile : NotifyPropertyChanged
    {
        [JsonProperty("Tablet")]
        public string? Tablet { set; get; }

        [JsonProperty("OutputMode")]
        public PluginSettings? OutputMode { set; get; }

        [JsonProperty("Filters")]
        public PluginSettingsCollection? Filters { set; get; }

        [JsonProperty("AbsoluteModeSettings")]
        public AbsoluteModeSettings? AbsoluteModeSettings { set; get; }

        [JsonProperty("RelativeModeSettings")]
        public RelativeModeSettings? RelativeModeSettings { set; get; }

        [JsonProperty("Bindings")]
        public BindingSettings? BindingSettings { set; get; }
    }
}
