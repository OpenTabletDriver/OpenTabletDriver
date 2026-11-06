using System;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class AbsoluteModeSettings : ViewModel
    {
        private bool _lockar, _areaLimiting;

        [JsonProperty(nameof(Display))]
        public required AreaSettings Display
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        [JsonProperty(nameof(Tablet))]
        public required AreaSettings Tablet
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        [JsonProperty(nameof(EnableClipping))]
        public bool EnableClipping
        {
            get;
            set => RaiseAndSetIfChanged(ref field, value);
        } = true;

        [JsonProperty(nameof(EnableAreaLimiting))]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref _areaLimiting, value);
            get => _areaLimiting;
        }

        [JsonProperty(nameof(LockAspectRatio))]
        public bool LockAspectRatio
        {
            set => RaiseAndSetIfChanged(ref _lockar, value);
            get => _lockar;
        }

        public static AbsoluteModeSettings GetDefaults(DigitizerSpecifications digitizer)
        {
            var display = AppInfo.PluginManager.GetService<IVirtualScreen>() ?? throw new InvalidOperationException($"Could not get {nameof(IVirtualScreen)} from DI");

            return new AbsoluteModeSettings
            {
                Display = AreaSettings.GetDefaults(display),
                Tablet = AreaSettings.GetDefaults(digitizer),
            };
        }
    }
}
