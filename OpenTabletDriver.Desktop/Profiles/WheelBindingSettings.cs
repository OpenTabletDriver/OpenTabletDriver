using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

#nullable enable

namespace OpenTabletDriver.Desktop.Profiles
{
    /// <summary>
    /// The settings for the wheel bindings of a single wheel
    /// </summary>
    public class WheelBindingSettings : ViewModel
    {
        // TODO: default values for these are current relied upon by UI StepSize behavior
        //       It should be instead be initialized by TabletReference or similar


        [JsonProperty(nameof(WheelButtons))]
        public PluginSettingStoreCollection WheelButtons
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(ClockwiseRotation))]
        public PluginSettingStore? ClockwiseRotation
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(ClockwiseActivationThreshold))]
        public float ClockwiseActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(CounterClockwiseRotation))]
        public PluginSettingStore? CounterClockwiseRotation
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(CounterClockwiseActivationThreshold))]
        public float CounterClockwiseActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        // should be instantiated by daemon on load
        [JsonProperty(nameof(StepSize))]
        public double? StepSize
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }
    }
}
