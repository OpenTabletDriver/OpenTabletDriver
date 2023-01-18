using System.ComponentModel;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings
    {

        [DisplayName("Tip Activation Threshold"), JsonProperty("TipActivationThreshold")]
        public float TipActivationThreshold { set; get; }

        [DisplayName("Tip Button"), JsonProperty("TipButton")]
        public PluginSettings? TipButton { set; get; }

        [DisplayName("Eraser Activation Threshold"), JsonProperty("EraserActivationThreshold")]
        public float EraserActivationThreshold { set; get; }

        [DisplayName("Eraser Button"), JsonProperty("EraserButton")]
        public PluginSettings? EraserButton { set; get; }

        [DisplayName("Pen Button"), JsonProperty("PenButtons")]
        public PluginSettingsCollection PenButtons { set; get; } = new();

        [DisplayName("Auxiliary Button"), JsonProperty("AuxButtons")]
        public PluginSettingsCollection AuxButtons { set; get; } = new();

        [DisplayName("Mouse Button"), JsonProperty("MouseButtons")]
        public PluginSettingsCollection MouseButtons { set; get; } = new();

        [DisplayName("Mouse Scroll Up"), JsonProperty("MouseScrollUp")]
        public PluginSettings? MouseScrollUp { set; get; }

        [DisplayName("Mouse Scroll Down"), JsonProperty("MouseScrollDown")]
        public PluginSettings? MouseScrollDown { set; get; }

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
