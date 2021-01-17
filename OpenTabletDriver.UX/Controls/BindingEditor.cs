using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows.Bindings;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingEditor : Panel
    {
        public BindingEditor()
        {
            this.Padding = 5;
            content.Orientation = Orientation.Vertical;
            content.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            UpdateBindings();
            App.SettingsChanged += (s) => UpdateBindings();
            base.Content = content;
        }

        private StackView content = new StackView();

        public async void UpdateBindings(TabletState tablet = null)
        {
            content.Items.Clear();

            tablet ??= await App.Driver.Instance.GetTablet();

            var tipButton = new BindingDisplay(App.Settings?.TipButton)
            {
                Width = 250
            };
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
                    new Group("Tip Button", tipButton, Orientation.Horizontal, false),
                    tipPressure
                }
            };

            var tipSettings = new Group("Tip Bindings", tipSettingsStack);

            var penBindingsStack = new StackView();
            for (int i = 0; i < (tablet?.Digitizer?.MaxPenButtonCount ?? 0); i++)
            {
                if (App.Settings?.PenButtons?.Count <= i)
                    App.Settings?.PenButtons?.Add(null);
                var penBinding = new BindingDisplay(App.Settings?.PenButtons[i])
                {
                    Width = 250,
                    Tag = i
                };
                penBinding.BindingUpdated += (sender, binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.PenButtons[index] = binding;
                };
                var penBindingGroup = new Group($"Pen Button {i + 1}", penBinding, Orientation.Horizontal, false);
                penBindingsStack.AddControl(penBindingGroup);
            }

            var penBindingSettings = new Group("Pen Button Bindings", penBindingsStack);

            var penSettings = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem(tipSettings, true),
                    new StackLayoutItem(penBindingSettings, true)
                }
            };
            content.AddControl(penSettings);

            var auxBindingsStack = new StackView();
            var auxCount = tablet?.Auxiliary?.ButtonCount ?? tablet?.Digitizer?.ButtonCount ?? 0;
            for (int i = 0; i < auxCount; i++)
            {
                if (App.Settings?.AuxButtons?.Count <= i)
                    App.Settings?.AuxButtons?.Add(null);
                var auxBinding = new BindingDisplay(App.Settings?.AuxButtons[i])
                {
                    Width = 250,
                    Tag = i
                };
                auxBinding.BindingUpdated += (sender, Binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.AuxButtons[index] = Binding;
                };
                var auxBindingGroup = new Group($"Auxiliary Button {i + 1}", auxBinding, Orientation.Horizontal, false);
                auxBindingsStack.AddControl(auxBindingGroup);
            }

            var auxBindingSettings = new Group("Auxiliary Button Bindings", auxBindingsStack)
            {
                TitleHorizontalAlignment = HorizontalAlignment.Center
            };
            content.AddControl(auxBindingSettings);
        }

        internal class BindingDisplay : Button
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

        private class PressureSlider : Group
        {
            public PressureSlider(
                string header,
                Func<float> getValue,
                Action<float> setValue
            )
            {
                this.Text = header;
                this.Orientation = Orientation.Horizontal;

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
