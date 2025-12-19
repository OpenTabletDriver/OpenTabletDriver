using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        private float tP = 1, eP = 1;
        private float ct = 15, cct = 15;
        private PluginSettingStore tipButton,
            eraserButton,
            mouseScrollUp,
            mouseScrollDown,
            clockwiseRotation,
            counterClockwiseRotation;

        private PluginSettingStoreCollection penButtons = new PluginSettingStoreCollection(),
            auxButtons = new PluginSettingStoreCollection(),
            mouseButtons = new PluginSettingStoreCollection(),
            wheelButtons = new PluginSettingStoreCollection();

        private bool disablePressure, disableTilt, confidentReportsOnly;

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

        [JsonProperty("DisablePressure")]
        public bool DisablePressure
        {
            set => this.RaiseAndSetIfChanged(ref this.disablePressure, value);
            get => this.disablePressure;
        }

        [JsonProperty("DisableTilt")]
        public bool DisableTilt
        {
            set => this.RaiseAndSetIfChanged(ref this.disableTilt, value);
            get => this.disableTilt;
        }

        [JsonProperty("ConfidentReportsOnly")]
        public bool ConfidentReportsOnly
        {
            set => RaiseAndSetIfChanged(ref this.confidentReportsOnly, value);
            get => this.confidentReportsOnly;
        }

        public static BindingSettings GetDefaults(TabletSpecifications tabletSpecifications)
        {
            var bindingSettings = new BindingSettings
            {
                TipButton = new PluginSettingStore(
                    new AdaptiveBinding(PenAction.Tip)
                ),
                EraserButton = new PluginSettingStore(
                    new AdaptiveBinding(PenAction.Eraser)
                ),
                PenButtons = new PluginSettingStoreCollection(),
                AuxButtons = new PluginSettingStoreCollection(),
                MouseButtons = new PluginSettingStoreCollection(),
                WheelButtons = new PluginSettingStoreCollection()
            };

            bindingSettings.AddPenButtons(tabletSpecifications);

            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }

        public void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;
            int wheelButtonCount = (int?)tabletSpecifications.Wheel?.Buttons.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);
            WheelButtons = WheelButtons.SetExpectedCount(wheelButtonCount);
        }

        private void AddPenButtons(TabletSpecifications tabletSpecifications)
        {
            uint buttonCount = tabletSpecifications.Pen.ButtonCount;
            if (buttonCount >= 1)
                PenButtons.Add(new PluginSettingStore(new AdaptiveBinding(PenAction.BarrelButton1)));
            if (buttonCount >= 2)
                PenButtons.Add(new PluginSettingStore(new AdaptiveBinding(PenAction.BarrelButton2)));
            if (buttonCount >= 3)
                PenButtons.Add(new PluginSettingStore(new AdaptiveBinding(PenAction.BarrelButton3)));
        }
    }
}
