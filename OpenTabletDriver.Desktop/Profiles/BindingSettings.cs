using System.ComponentModel;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : NotifyPropertyChanged
    {
        private float _tP, _eP;
        private PluginSettings _tipButton, _eraserButton, _mouseScrollUp, _mouseScrollDown;
        private PluginSettingsCollection _penButtons = new PluginSettingsCollection(),
            _auxButtons = new PluginSettingsCollection(),
            _mouseButtons = new PluginSettingsCollection();

        [DisplayName("Tip Activation Threshold"), JsonProperty("TipActivationThreshold")]
        public float TipActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _tP, value);
            get => _tP;
        }

        [DisplayName("Tip Button"), JsonProperty("TipButton")]
        public PluginSettings TipButton
        {
            set => RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        [DisplayName("Eraser Activation Threshold"), JsonProperty("EraserActivationThreshold")]
        public float EraserActivationThreshold
        {
            set => RaiseAndSetIfChanged(ref _eP, value);
            get => _eP;
        }

        [DisplayName("Eraser Button"), JsonProperty("EraserButton")]
        public PluginSettings EraserButton
        {
            set => RaiseAndSetIfChanged(ref _eraserButton, value);
            get => _eraserButton;
        }

        [DisplayName("Pen Button"), JsonProperty("PenButtons")]
        public PluginSettingsCollection PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        [DisplayName("Auxiliary Button"), JsonProperty("AuxButtons")]
        public PluginSettingsCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        [DisplayName("Mouse Button"), JsonProperty("MouseButtons")]
        public PluginSettingsCollection MouseButtons
        {
            set => RaiseAndSetIfChanged(ref _mouseButtons, value);
            get => _mouseButtons;
        }

        [DisplayName("Mouse Scroll Up"), JsonProperty("MouseScrollUp")]
        public PluginSettings MouseScrollUp
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollUp, value);
            get => _mouseScrollUp;
        }

        [DisplayName("Mouse Scroll Down"), JsonProperty("MouseScrollDown")]
        public PluginSettings MouseScrollDown
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollDown, value);
            get => _mouseScrollDown;
        }

        public static BindingSettings GetDefaults(TabletSpecifications tabletSpecifications)
        {
            var bindingSettings = new BindingSettings
            {
                TipButton = new PluginSettings(
                    typeof(MouseBinding),
                    new
                    {
                        Button = nameof(MouseButton.Left)
                    }
                ),
                PenButtons = new PluginSettingsCollection(),
                AuxButtons = new PluginSettingsCollection(),
                MouseButtons = new PluginSettingsCollection()
            };
            bindingSettings.MatchSpecifications(tabletSpecifications);
            return bindingSettings;
        }

        private void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);
        }
    }
}
