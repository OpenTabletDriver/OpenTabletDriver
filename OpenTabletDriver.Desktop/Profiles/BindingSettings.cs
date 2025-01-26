using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        private float tP, eP, ct, cct;

        private PluginSettingStore tipButton, eraserButton, mouseScrollUp, mouseScrollDown, clockwiseRotation, counterClockwiseRotation;
        private PluginSettingStoreCollection penButtons = new PluginSettingStoreCollection(),
            auxButtons = new PluginSettingStoreCollection(),
            mouseButtons = new PluginSettingStoreCollection(),
            wheelButtons = new PluginSettingStoreCollection();

        [JsonProperty("TipActivationThreshold")]
        public float TipActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref this.tP, value);
            get => this.tP;
        }

        [JsonProperty("TipButton")]
        public PluginSettingStore TipButton
        {
            set => this.RaiseAndSetIfChanged(ref this.tipButton, value);
            get => this.tipButton;
        }

        [JsonProperty("EraserActivationThreshold")]
        public float EraserActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref this.eP, value);
            get => this.eP;
        }

        [JsonProperty("EraserButton")]
        public PluginSettingStore EraserButton
        {
            set => this.RaiseAndSetIfChanged(ref this.eraserButton, value);
            get => this.eraserButton;
        }

        [JsonProperty("PenButtons")]
        public PluginSettingStoreCollection PenButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.penButtons, value);
            get => this.penButtons;
        }

        [JsonProperty("AuxButtons")]
        public PluginSettingStoreCollection AuxButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.auxButtons, value);
            get => this.auxButtons;
        }

        [JsonProperty("MouseButtons")]
        public PluginSettingStoreCollection MouseButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.mouseButtons, value);
            get => this.mouseButtons;
        }

        [JsonProperty("WheelButtons")]
        public PluginSettingStoreCollection WheelButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.wheelButtons, value);
            get => this.wheelButtons;
        }

        [JsonProperty("MouseScrollUp")]
        public PluginSettingStore MouseScrollUp
        {
            set => this.RaiseAndSetIfChanged(ref this.mouseScrollUp, value);
            get => this.mouseScrollUp;
        }

        [JsonProperty("MouseScrollDown")]
        public PluginSettingStore MouseScrollDown
        {
            set => this.RaiseAndSetIfChanged(ref this.mouseScrollDown, value);
            get => this.mouseScrollDown;
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

        public static BindingSettings GetDefaults(TabletSpecifications tabletSpecifications)
        {
            var bindingSettings = new BindingSettings
            {
                TipButton = new PluginSettingStore(
                    new MouseBinding
                    {
                        Button = nameof(MouseButton.Left)
                    }
                ),
                PenButtons = new PluginSettingStoreCollection(),
                AuxButtons = new PluginSettingStoreCollection(),
                MouseButtons = new PluginSettingStoreCollection(),
                WheelButtons = new PluginSettingStoreCollection()
            };
            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }

        public void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.Buttons?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;
            int wheelButtonCount = (int?)tabletSpecifications.Wheel?.Buttons.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);
            WheelButtons = WheelButtons.SetExpectedCount(wheelButtonCount);
        }
    }
}
