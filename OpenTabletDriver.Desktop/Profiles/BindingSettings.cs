using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        public const int PEN_BUTTON_MAX = 2;
        public const int AUX_BUTTON_MAX = 8;

        private float tP, eP;
        private PluginSettingStore tipButton, eraserButton;
        private PluginSettingStoreCollection penButtons = new PluginSettingStoreCollection(),
            auxButtons = new PluginSettingStoreCollection();

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => RaiseAndSetIfChanged(ref this.tP, value);
            get => this.tP;
        }

        [JsonProperty("TipButton")]
        public PluginSettingStore TipButton
        {
            set => RaiseAndSetIfChanged(ref this.tipButton, value);
            get => this.tipButton;
        }

        [JsonProperty("EraserActivationPressure")]
        public float EraserActivationPressure
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
            set => RaiseAndSetIfChanged(ref this.penButtons, value);
            get => this.penButtons;
        }

        [JsonProperty("AuxButtons")]
        public PluginSettingStoreCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref this.auxButtons, value);
            get => this.auxButtons;
        }

        public static BindingSettings GetDefaults()
        {
            return new BindingSettings
            {
                TipButton = new PluginSettingStore(
                    new MouseBinding
                    {
                        Button = nameof(MouseButton.Left)
                    }
                ),
                PenButtons = new PluginSettingStoreCollection().SetExpectedCount(PEN_BUTTON_MAX),
                AuxButtons = new PluginSettingStoreCollection().SetExpectedCount(AUX_BUTTON_MAX)
            };
        }
    }
}