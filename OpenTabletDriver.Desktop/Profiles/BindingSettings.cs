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
        private PluginSettings? _tipButton, _eraserButton,
            _mouseScrollUp, _mouseScrollDown,
            _wheelClockwise, _wheelCounterClockwise, _wheelTouch;
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
        public PluginSettings? TipButton
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
        public PluginSettings? EraserButton
        {
            set => RaiseAndSetIfChanged(ref _eraserButton, value);
            get => _eraserButton;
        }

        [DisplayName("Pen Button"), JsonProperty("PenButtons")]
        public PluginSettingsCollection PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons!, value);
            get => _penButtons;
        }

        [DisplayName("Auxiliary Button"), JsonProperty("AuxButtons")]
        public PluginSettingsCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons!, value);
            get => _auxButtons;
        }

        [DisplayName("Mouse Button"), JsonProperty("MouseButtons")]
        public PluginSettingsCollection MouseButtons
        {
            set => RaiseAndSetIfChanged(ref _mouseButtons!, value);
            get => _mouseButtons;
        }

        [DisplayName("Mouse Scroll Up"), JsonProperty("MouseScrollUp")]
        public PluginSettings? MouseScrollUp
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollUp, value);
            get => _mouseScrollUp;
        }

        [DisplayName("Mouse Scroll Down"), JsonProperty("MouseScrollDown")]
        public PluginSettings? MouseScrollDown
        {
            set => RaiseAndSetIfChanged(ref _mouseScrollDown, value);
            get => _mouseScrollDown;
        }

        [DisplayName("Wheel Clockwise"), JsonProperty("WheelClockwise")]
        public PluginSettings? WheelClockwise
        {
            set => RaiseAndSetIfChanged(ref _wheelClockwise, value);
            get => _wheelClockwise;
        }

        [DisplayName("Wheel Counterclockwise"), JsonProperty("WheelCounterClockwise")]
        public PluginSettings? WheelCounterClockwise
        {
            set => RaiseAndSetIfChanged(ref _wheelCounterClockwise, value);
            get => _wheelCounterClockwise;
        }

        [DisplayName("Wheel Touch"), JsonProperty("WheelTouch")]
        public PluginSettings? WheelTouch
        {
            set => RaiseAndSetIfChanged(ref _wheelTouch, value);
            get => _wheelTouch;
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
            var penButtonCount = tabletSpecifications.Pen?.ButtonCount ?? 0;
            var auxButtonCount = tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            var mouseButtonCount = tabletSpecifications.MouseButtons?.ButtonCount ?? 0;

            PenButtons = PenButtons.SetLength(penButtonCount);
            AuxButtons = AuxButtons.SetLength(auxButtonCount);
            MouseButtons = MouseButtons.SetLength(mouseButtonCount);
        }
    }
}
