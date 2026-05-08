using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class AbsoluteModeSettings : ViewModel
    {
        [JsonProperty(nameof(Display))]
        public AreaSettings Display
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(Tablet))]
        public AreaSettings Tablet
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(EnableClipping))]
        public bool EnableClipping
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(EnableAreaLimiting))]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(LockAspectRatio))]
        public bool LockAspectRatio
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        public static AbsoluteModeSettings GetDefaults(DigitizerSpecifications digitizer)
        {
            var display = AppInfo.PluginManager.GetService<IVirtualScreen>();

            return new AbsoluteModeSettings
            {
                Display = AreaSettings.GetDefaults(display),
                Tablet = AreaSettings.GetDefaults(digitizer),
                EnableClipping = true
            };
        }
    }
}
