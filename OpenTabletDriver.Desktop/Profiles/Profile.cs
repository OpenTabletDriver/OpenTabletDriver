using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class Profile : NotifyPropertyChanged
    {
        private string _tablet = string.Empty;
        private int _persistentId;
        private PluginSettings _outputMode = null!;
        private BindingSettings _bindings = new BindingSettings();
        private PluginSettingsCollection _filters = new PluginSettingsCollection();

        [JsonProperty("Tablet")]
        public string Tablet
        {
            set => RaiseAndSetIfChanged(ref _tablet!, value);
            get => _tablet;
        }

        [JsonProperty("PersistentId")]
        public int PersistentId
        {
            set => RaiseAndSetIfChanged(ref _persistentId, value);
            get => _persistentId;
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
        public BindingSettings Bindings
        {
            set => RaiseAndSetIfChanged(ref _bindings!, value);
            get => _bindings;
        }

        public static Profile GetDefaults(IServiceProvider serviceProvider, InputDevice tablet)
        {
            var screen = serviceProvider.GetRequiredService<IVirtualScreen>();
            var digitizer = tablet.Configuration.Specifications.Digitizer;

            return new Profile
            {
                Tablet = tablet.Configuration.Name,
                PersistentId = tablet.PersistentId!.Value,
                OutputMode = serviceProvider.GetDefaultSettings(typeof(AbsoluteMode), digitizer!, screen),
                Bindings = BindingSettings.GetDefaults(tablet.Configuration.Specifications)
            };
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Tablet;
        }
    }
}
