using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class Profile : ViewModel
    {
        private string tablet;
        private PluginSettingStore outputMode;
        private AbsoluteModeSettings absoluteMode = new AbsoluteModeSettings();
        private RelativeModeSettings relativeMode = new RelativeModeSettings();
        private BindingSettings bindings = new BindingSettings();
        private PluginSettingStoreCollection filters = new PluginSettingStoreCollection();

        [JsonProperty("Tablet")]
        public string Tablet
        {
            set => this.RaiseAndSetIfChanged(ref tablet, value);
            get => tablet;
        }

        [JsonProperty("OutputMode")]
        public PluginSettingStore OutputMode
        {
            set => RaiseAndSetIfChanged(ref outputMode, value);
            get => outputMode;
        }

        [JsonProperty("Filters")]
        public PluginSettingStoreCollection Filters
        {
            set => RaiseAndSetIfChanged(ref filters, value);
            get => filters;
        }

        [JsonProperty("AbsoluteModeSettings")]
        public AbsoluteModeSettings AbsoluteModeSettings
        {
            set => this.RaiseAndSetIfChanged(ref absoluteMode, value);
            get => absoluteMode;
        }

        [JsonProperty("RelativeModeSettings")]
        public RelativeModeSettings RelativeModeSettings
        {
            set => this.RaiseAndSetIfChanged(ref relativeMode, value);
            get => relativeMode;
        }

        [JsonProperty("Bindings")]
        public BindingSettings BindingSettings
        {
            set => this.RaiseAndSetIfChanged(ref bindings, value);
            get => bindings;
        }

        public static Profile GetDefaults(TabletReference tablet)
        {
            return new Profile
            {
                Tablet = tablet.Properties.Name,
                OutputMode = new PluginSettingStore(typeof(AbsoluteMode)),
                AbsoluteModeSettings = AbsoluteModeSettings.GetDefaults(tablet.Properties.Specifications.Digitizer),
                RelativeModeSettings = RelativeModeSettings.GetDefaults(),
                BindingSettings = BindingSettings.GetDefaults(tablet.Properties.Specifications)
            };
        }
    }
}
