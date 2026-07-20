using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Profiles
{
    /// <summary>
    /// The settings for the wheel bindings of a single wheel
    /// </summary>
    public class WheelBindingSettings : ViewModel
    {
        private PluginSettingStoreCollection _wheelButtons = [];

        // TODO: default values for these are current relied upon by UI StepSize behavior
        //       It should be instead be initialized by TabletReference or similar
        private float _ct, _cct;

        private PluginSettingStore?
            _clockwiseRotation,
            _counterClockwiseRotation;

        private double? _stepSize;

        [JsonProperty(nameof(WheelButtons))]
        public PluginSettingStoreCollection WheelButtons
        {
            set => RaiseAndSetIfChanged(ref _wheelButtons, value);
            get => _wheelButtons;
        }

        [JsonProperty(nameof(ClockwiseRotation))]
        public PluginSettingStore? ClockwiseRotation
        {
            set => RaiseAndSetIfChanged(ref _clockwiseRotation, value);
            get => _clockwiseRotation;
        }

        [JsonProperty(nameof(ClockwiseActivationThreshold))]
        public float ClockwiseActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _ct, value);
            get => _ct;
        }

        [JsonProperty(nameof(CounterClockwiseRotation))]
        public PluginSettingStore? CounterClockwiseRotation
        {
            set => RaiseAndSetIfChanged(ref _counterClockwiseRotation, value);
            get => _counterClockwiseRotation;
        }

        [JsonProperty(nameof(CounterClockwiseActivationThreshold))]
        public float CounterClockwiseActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _cct, value);
            get => _cct;
        }

        // should be instantiated by daemon on load
        [JsonProperty(nameof(StepSize))]
        public double? StepSize
        {
            set => RaiseAndSetIfChanged(ref _stepSize, value);
            get => _stepSize;
        }
    }
}
