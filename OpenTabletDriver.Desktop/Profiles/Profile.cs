using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class Profile : NotifyPropertyChanged
    {
        private string _tablet = string.Empty;
        private PluginSettings _outputMode = null!;
        private BindingSettings _bindings = new BindingSettings();
        private PluginSettingsCollection _filters = new PluginSettingsCollection();

        [JsonProperty("Tablet")]
        public string Tablet
        {
            set => RaiseAndSetIfChanged(ref _tablet!, value);
            get => _tablet;
        }

        [JsonProperty("OutputMode")]
        public PluginSettings OutputMode
        {
            set => RaiseAndSetIfChanged(ref _outputMode!, value);
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public PluginSettingsCollection Filters
        {
            set => RaiseAndSetIfChanged(ref _filters!, value);
            get => _filters;
        }

        [JsonProperty("Bindings")]
        public BindingSettings BindingSettings
        {
            set => RaiseAndSetIfChanged(ref _bindings!, value);
            get => _bindings;
        }

        private static Type DefaultOutputModeType =>
            SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Linux => typeof(LinuxArtistMode),
                _ => typeof(AbsoluteMode)
            };

        public static Profile GetDefaults(IServiceProvider serviceProvider, InputDevice tablet)
        {
            var screen = serviceProvider.GetRequiredService<IVirtualScreen>();
            var digitizer = tablet.Configuration.Specifications.Digitizer;

            return new Profile
            {
                Tablet = tablet.Configuration.Name,
                OutputMode = serviceProvider.GetDefaultSettings(DefaultOutputModeType, digitizer!, screen),
                BindingSettings = BindingSettings.GetDefaults(tablet.Configuration.Specifications)
            };
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Tablet;
        }
    }
}
