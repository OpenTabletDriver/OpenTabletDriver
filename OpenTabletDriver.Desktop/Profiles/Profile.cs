using System;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class Profile : ViewModel
    {
        [JsonProperty(nameof(Tablet))]
        public string Tablet
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(OutputMode))]
        public PluginSettingStore OutputMode
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(Filters))]
        public PluginSettingStoreCollection Filters
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(AbsoluteModeSettings))]
        public AbsoluteModeSettings AbsoluteModeSettings
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = new AbsoluteModeSettings();

        [JsonProperty(nameof(RelativeModeSettings))]
        public RelativeModeSettings RelativeModeSettings
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = new RelativeModeSettings();

        [JsonProperty("Bindings")]
        public BindingSettings BindingSettings
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = new BindingSettings();

        private static Type DefaultOutputModeType =>
            SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Linux => typeof(LinuxArtistMode),
                _ => typeof(AbsoluteMode)
            };

        public static Profile GetDefaults(TabletReference tablet)
        {
            return new Profile
            {
                Tablet = tablet.Properties.Name,
                OutputMode = new PluginSettingStore(DefaultOutputModeType),
                AbsoluteModeSettings = AbsoluteModeSettings.GetDefaults(tablet.Properties.Specifications.Digitizer),
                RelativeModeSettings = RelativeModeSettings.GetDefaults(),
                BindingSettings = BindingSettings.GetDefaults(tablet.Properties.Specifications)
            };
        }
    }
}
