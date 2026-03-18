using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class BindingSettings : ViewModel
    {
        [JsonProperty(nameof(TipActivationThreshold))]
        public float TipActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = 1;

        [JsonProperty(nameof(TipButton))]
        public PluginSettingStore TipButton
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(EraserActivationThreshold))]
        public float EraserActivationThreshold
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = 1;

        [JsonProperty(nameof(EraserButton))]
        public PluginSettingStore EraserButton
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(PenButtons))]
        public PluginSettingStoreCollection PenButtons
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(AuxButtons))]
        public PluginSettingStoreCollection AuxButtons
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(MouseButtons))]
        public PluginSettingStoreCollection MouseButtons
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(MouseScrollUp))]
        public PluginSettingStore MouseScrollUp
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(MouseScrollDown))]
        public PluginSettingStore MouseScrollDown
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(WheelBindings))]
        public List<WheelBindingSettings> WheelBindings
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        } = [];

        [JsonProperty(nameof(DisablePressure))]
        public bool DisablePressure
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(DisableTilt))]
        public bool DisableTilt
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(EnableDragBindings))]
        public bool EnableDragBindings
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
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
                PenButtons = [],
                AuxButtons = [],
                MouseButtons = [],
            };

            bindingSettings.AddPenButtons(tabletSpecifications);

            bindingSettings.MatchSpecifications(tabletSpecifications);

            bindingSettings.SetupWheelDefaults(tabletSpecifications);

            return bindingSettings;
        }

        public void MatchSpecifications(TabletSpecifications tabletSpecifications)
        {
            int penButtonCount = (int?)tabletSpecifications.Pen?.ButtonCount ?? 0;
            int auxButtonCount = (int?)tabletSpecifications.AuxiliaryButtons?.ButtonCount ?? 0;
            int mouseButtonCount = (int?)tabletSpecifications.MouseButtons?.ButtonCount ?? 0;

            PenButtons = PenButtons.SetExpectedCount(penButtonCount);
            AuxButtons = AuxButtons.SetExpectedCount(auxButtonCount);
            MouseButtons = MouseButtons.SetExpectedCount(mouseButtonCount);

            MatchWheelSpecifications(tabletSpecifications);
        }

        private void MatchWheelSpecifications(TabletSpecifications tabletSpecifications)
        {
            int wheelCount = tabletSpecifications.Wheels?.Count ?? 0;
            int trimmed = 0;

            while (WheelBindings.Count > wheelCount)
            {
                trimmed++;
                WheelBindings.RemoveAt(WheelBindings.Count - 1);
            }

            for (int i = WheelBindings.Count; i < wheelCount; i++)
            {
                var wheelBindingSettings = new WheelBindingSettings();
                WheelBindings.Add(wheelBindingSettings);
            }

            for (int i = 0; i < wheelCount; i++)
            {
                var wheelBinding = WheelBindings[i];
                wheelBinding.StepSize = GetDegreesPerStep(tabletSpecifications, i);

                int buttonCountForWheel = (int)tabletSpecifications.Wheels![i].ButtonCount;
                wheelBinding.WheelButtons.SetExpectedCount(buttonCountForWheel);

                wheelBinding.ClockwiseActivationThreshold =
                    VerifyWheelActivationThreshold(wheelBinding.ClockwiseActivationThreshold);

                wheelBinding.CounterClockwiseActivationThreshold =
                    VerifyWheelActivationThreshold(wheelBinding.CounterClockwiseActivationThreshold);
            }

            if (trimmed > 0)
                Log.WriteNotify(nameof(BindingSettings), $"Too many wheels were configured. Trimmed the last {trimmed} wheel configuration(s) out.", LogLevel.Warning);

            Debug.Assert(WheelBindings.Count == wheelCount, "WheelBindings.Count != wheelCount");
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

        private void SetupWheelDefaults(TabletSpecifications tabletSpecifications)
        {
            if (tabletSpecifications.Wheels == null) return;

            Debug.Assert(WheelBindings.Count == tabletSpecifications.Wheels.Count);

            for (int i = 0; i < tabletSpecifications.Wheels.Count; i++)
            {
                var wheelBinding = WheelBindings[i];
                var wheelSpecification = tabletSpecifications.Wheels[i];

                if (wheelSpecification.StepCount == null)
                {
                    Log.Write(nameof(BindingSettings), "Unable to determine wheel step count");
                    continue;
                }

                wheelBinding.ClockwiseActivationThreshold =
                    wheelBinding.CounterClockwiseActivationThreshold = (float)(360d / wheelSpecification.StepCount);

                // TODO: Figure out good default settings for rotations
                // wheelBinding.ClockwiseRotation = new PluginSettingStore(new KeyBinding { Key = "PageDown" });
                // wheelBinding.CounterClockwiseRotation = new PluginSettingStore(new KeyBinding { Key = "PageUp" });
            }
        }

        private static double GetDegreesPerStep(TabletSpecifications spec, int wheelIndex)
        {
            ArgumentNullException.ThrowIfNull(spec);

            if (spec.Wheels != null && spec.Wheels.Count >= wheelIndex && spec.Wheels[wheelIndex].StepCount != null)
                return 360d / spec.Wheels[wheelIndex].StepCount!.Value;

            throw new InvalidOperationException("Provided TabletSpecifications does not define wheel step count for this wheel");
        }

        private static float VerifyWheelActivationThreshold(float threshold)
        {
            if (threshold > 0) return threshold;

            Log.Write(nameof(BindingSettings), $"Forcing invalid wheel threshold value '{threshold}' to 1");
            return 1;
        }
    }
}
