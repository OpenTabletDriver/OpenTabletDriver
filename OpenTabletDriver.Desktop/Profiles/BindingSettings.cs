using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        private float tP = 1;
        private float eP = 1;
        private PluginSettingStore tipButton, eraserButton, mouseScrollUp, mouseScrollDown;
        private PluginSettingStoreCollection penButtons = new PluginSettingStoreCollection(),
            auxButtons = new PluginSettingStoreCollection(),
            touchStrips = new PluginSettingStoreCollection(),
            mouseButtons = new PluginSettingStoreCollection();

        private bool disablePressure, disableTilt;

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

        [JsonProperty("TouchStrips")]
        public PluginSettingStoreCollection TouchStrips
        {
            set => this.RaiseAndSetIfChanged(ref this.touchStrips, value);
            get => this.touchStrips;
        }

        [JsonProperty("MouseButtons")]
        public PluginSettingStoreCollection MouseButtons
        {
            set => this.RaiseAndSetIfChanged(ref this.mouseButtons, value);
            get => this.mouseButtons;
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
                TouchStrips = new PluginSettingStoreCollection(),
                MouseButtons = new PluginSettingStoreCollection(),
            };
            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }

        public void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int touchStripsCount = (int?)tabletSpecifications.TouchStrips?.Count ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            TouchStrips = TouchStrips.SetExpectedCount(touchStripsCount * 2);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);
        }
    }
}
