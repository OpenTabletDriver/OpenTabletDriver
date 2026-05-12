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
        private AbsoluteModeSettings? absoluteMode;
        private RelativeModeSettings? relativeMode;
        private BindingSettings bindings = new BindingSettings();
        private PluginSettingStoreCollection filters = new PluginSettingStoreCollection();

        [JsonProperty(nameof(Tablet))]
        public required string Tablet
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        [JsonProperty(nameof(OutputMode))]
        public required PluginSettingStore OutputMode
        {
            get;
            set => RaiseAndSetIfChanged(ref field, value);
        }

        [JsonProperty(nameof(Filters))]
        public PluginSettingStoreCollection Filters
        {
            set => RaiseAndSetIfChanged(ref filters, value);
            get => filters;
        }

        [JsonProperty(nameof(AbsoluteModeSettings))]
        public AbsoluteModeSettings? AbsoluteModeSettings
        {
            set => this.RaiseAndSetIfChanged(ref absoluteMode, value);
            get => absoluteMode;
        }

        [JsonProperty(nameof(RelativeModeSettings))]
        public RelativeModeSettings? RelativeModeSettings
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
