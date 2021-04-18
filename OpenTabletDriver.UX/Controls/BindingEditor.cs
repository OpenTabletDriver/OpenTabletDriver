using System;
using System.Linq;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
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

        public void UpdateBindings()
        {
            content.Items.Clear();

            var tipButton = new BindingDisplay(App.Settings?.TipButton)
            {
                MinimumSize = new Size(300, 0)
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
            for (int i = 0; i < App.Settings?.PenButtons.Count; i++)
            {
                var penBinding = new BindingDisplay(App.Settings?.PenButtons[i])
                {
                    MinimumSize = new Size(300, 0),
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
            for (int i = 0; i < App.Settings?.AuxButtons.Count; i++)
            {
                var auxBinding = new BindingDisplay(App.Settings?.AuxButtons[i])
                {
                    MinimumSize = new Size(300, 0),
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

        internal class BindingDisplay : StackLayout
        {
            public BindingDisplay(PluginSettingStore store)
            {
                var bindingCommand = new Command();
                bindingCommand.Executed += async (sender, e) =>
                {
                    var dialog = new BindingEditorDialog(Binding);
                    this.Binding = await dialog.ShowModalAsync(this);
                };
                var advancedBindingCommand = new Command();
                advancedBindingCommand.Executed += async (sender, e) =>
                {
                    var dialog = new AdvancedBindingEditorDialog(Binding);
                    this.Binding = await dialog.ShowModalAsync(this);
                };

                mainButton = new Button
                {
                    Command = bindingCommand
                };
                advancedButton = new Button
                {
                    Command = advancedBindingCommand,
                    Text = "...",
                    Width = 25
                };

                Spacing = 5;
                Orientation = Orientation.Horizontal;
                Items.Add(new StackLayoutItem(mainButton, true));
                Items.Add(advancedButton);

                this.Binding = store;
            }

            public event EventHandler<PluginSettingStore> BindingUpdated;

            private Button mainButton, advancedButton;
            private PluginSettingStore binding;
            public PluginSettingStore Binding
            {
                set
                {
                    this.binding = value;
                    mainButton.Text = GetFriendlyDisplayString(Binding);
                    BindingUpdated?.Invoke(this, Binding);
                }
                get => this.binding;
            }

            private string GetFriendlyDisplayString(PluginSettingStore store)
            {
                if (store == null)
                    return null;

                var name = store.GetPluginReference().Name;
                string settings = string.Join(", ", store.Settings.Select(s => $"({s.Property}: {s.Value})"));

                return $"{name}: {settings}";
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
