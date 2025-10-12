using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class WheelSettings : ViewModel
    {
        private float ct = 15, cct = 15;
        private PluginSettingStore clockwiseRotation,
            counterClockwiseRotation;

        private PluginSettingStoreCollection wheelButtons = new PluginSettingStoreCollection();

        [JsonProperty("WheelButtons")]
        public PluginSettingStoreCollection WheelButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.wheelButtons, value);
            get => this.wheelButtons;
        }

        [JsonProperty("ClockwiseRotation")]
        public PluginSettingStore ClockwiseRotation
        {
            set => this.RaiseAndSetIfChanged(ref this.clockwiseRotation, value);
            get => this.clockwiseRotation;
        }

        [JsonProperty("ClockwiseActivationThreshold")]
        public float ClockwiseActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref this.ct, value);
            get => this.ct;
        }

        [JsonProperty("CounterClockwiseRotation")]
        public PluginSettingStore CounterClockwiseRotation
        {
            set => this.RaiseAndSetIfChanged(ref this.counterClockwiseRotation, value);
            get => this.counterClockwiseRotation;
        }

        [JsonProperty("CounterClockwiseActivationThreshold")]
        public float CounterClockwiseActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref this.cct, value);
            get => this.cct;
        }

        public static WheelSettings GetDefaults(TabletSpecifications tabletSpecifications)
        {
            var bindingSettings = new WheelSettings
            {

                WheelButtons = new PluginSettingStoreCollection()
            };
            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }
        
        public void MatchSpecifications(TabletSpecifications tabletSpecifications, int index = 0)
        {
            int wheelButtonCount = (int?)tabletSpecifications.Wheels[index]?.Buttons.ButtonCount ?? 0;

            WheelButtons = WheelButtons.SetExpectedCount(wheelButtonCount);
        }
    }
}