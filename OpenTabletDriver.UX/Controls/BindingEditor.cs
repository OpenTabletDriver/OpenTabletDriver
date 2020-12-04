using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingEditor : GroupBoxBase
    {
        public BindingEditor()
        {
            content.Orientation = Orientation.Horizontal;
            content.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            UpdateBindings();
            App.SettingsChanged += (s) => UpdateBindings();
            base.Content = content;
        }

        private StackView content = new StackView();

        public void UpdateBindings()
        {
            content.Items.Clear();

            var tipButton = new BindingDisplay(App.Settings?.TipButton);
            tipButton.BindingUpdated += (sender, binding) => App.Settings.TipButton = binding;

            var tipPressure = new PressureSlider(
                "Tip Pressure",
                () => App.Settings.TipActivationPressure,
                (v) => App.Settings.TipActivationPressure = v
            );

            var tipSettingsStack = new StackView
            {
                Items =
                {
                    tipButton,
                    tipPressure
                }
            };

            var tipSettings = new GroupBoxBase("Tip Bindings", tipSettingsStack);
            content.AddControl(tipSettings, true);

            var penBindingsStack = new StackView();
            for (int i = 0; i < App.Settings?.PenButtons.Count; i++)
            {
                var penBinding = new BindingDisplay(App.Settings?.PenButtons[i])
                {
                    Tag = i
                };
                penBinding.BindingUpdated += (sender, binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.PenButtons[index] = binding;
                };
                var penBindingGroup = new GroupBoxBase($"Pen Button {i + 1}", penBinding);
                penBindingsStack.AddControl(penBindingGroup);
            }

            var penBindingSettings = new GroupBoxBase("Pen Button Bindings", penBindingsStack);
            content.AddControl(penBindingSettings, true);

            var auxBindingsStack = new StackView();
            for (int i = 0; i < App.Settings?.AuxButtons.Count; i++)
            {
                var auxBinding = new BindingDisplay(App.Settings?.AuxButtons[i])
                {
                    Tag = i
                };
                auxBinding.BindingUpdated += (sender, Binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.AuxButtons[index] = Binding;
                };
                var auxBindingGroup = new GroupBoxBase($"Auxiliary Button {i + 1}", auxBinding);
                auxBindingsStack.AddControl(auxBindingGroup);
            }

            var auxBindingSettings = new GroupBoxBase("Auxiliary Button Bindings", auxBindingsStack);
            content.AddControl(auxBindingSettings, true);
        }

        private class BindingDisplay : Button
        {
            public BindingDisplay(PluginSettingStore store)
            {
                this.Binding = store;

                var bindingCommand = new Command();
                bindingCommand.Executed += async (sender, e) =>
                {
                    var dialog = new BindingEditorDialog(Binding);
                    this.Binding = await dialog.ShowModalAsync(this);
                };
                this.Command = bindingCommand;

                this.MouseDown += async (s, e) =>
                {
                    if (e.Buttons.HasFlag(MouseButtons.Alternate))
                    {
                        var dialog = new AdvancedBindingEditorDialog(Binding);
                        this.Binding = await dialog.ShowModalAsync(this);
                    }
                };
            }

            public event EventHandler<PluginSettingStore> BindingUpdated;

            private PluginSettingStore binding;
            public PluginSettingStore Binding
            {
                set
                {
                    this.binding = value;
                    Text = GetFriendlyDisplayString(Binding);
                    BindingUpdated?.Invoke(this, Binding);
                }
                get => this.binding;
            }

            private string GetFriendlyDisplayString(PluginSettingStore store)
            {
                if (store == null || store["Property"] == null)
                    return null;

                var property = store["Property"].GetValue<string>();
                var name = store.GetPluginReference().Name;

                return $"{name}: {property}";
            }
        }

        private class PressureSlider : GroupBoxBase
        {
            public PressureSlider(
                string header,
                Func<float> getValue,
                Action<float> setValue
            )
            {
                base.Text = header;

                this.setValue = setValue;
                this.getValue = getValue;

                var pressureslider = new Slider
                {
                    MinValue = 0,
                    MaxValue = 100
                };
                var fineTune = new TextBox();

                pressureslider.ValueChanged += (sender, e) =>
                {
                    this.setValue(pressureslider.Value);
                    fineTune.Text = this.getValue().ToString();
                    fineTune.CaretIndex = fineTune.Text.Length;
                };

                fineTune.TextChanged += (sender, e) =>
                {
                    var newValue = float.TryParse(fineTune.Text, out var val) ? val : 0f;
                    this.setValue(newValue);
                    pressureslider.Value = (int)this.getValue();
                };
                fineTune.Text = App.Settings?.TipActivationPressure.ToString();

                content.AddControl(pressureslider, true);
                content.AddControl(fineTune);
                base.Content = content;
            }

            private Action<float> setValue;
            private Func<float> getValue;

            private StackView content = new StackView
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }
    }
}
