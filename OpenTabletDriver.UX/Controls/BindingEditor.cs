using System;
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
            Padding = 5;
            UpdateBindings();
            App.SettingsChanged += (s) => UpdateBindings();
        }

        public void UpdateBindings()
        {
            BindingDisplay tipButton, eraserButton;

            var tipSettingsStack = new StackView
            {
                Items =
                {
                    new Group
                    {
                        Text = "Tip Button",
                        Orientation = Orientation.Horizontal,
                        ExpandContent = false,
                        Content = tipButton = new BindingDisplay(App.Settings?.TipButton)
                        {
                            MinimumSize = new Size(300, 0)
                        }
                    },
                    new PressureSlider(
                        "Tip Pressure",
                        () => App.Settings.TipActivationPressure,
                        (v) => App.Settings.TipActivationPressure = v
                    )
                }
            };
            tipButton.BindingUpdated += (sender, binding) => App.Settings.TipButton = binding;

            var eraserSettingsStack = new StackView
            {
                Items =
                {
                    new Group
                    {
                        Text = "Eraser Button",
                        ExpandContent = false,
                        Orientation = Orientation.Horizontal,
                        Content = eraserButton = new BindingDisplay(App.Settings?.EraserButton)
                        {
                            MinimumSize = new Size(300, 0)
                        }
                    },
                    new PressureSlider(
                        "Eraser Pressure",
                        () => App.Settings.EraserActivationPressure,
                        (v) => App.Settings.EraserActivationPressure = v
                    )
                }
            };
            eraserButton.BindingUpdated += (sender, binding) => App.Settings.EraserButton = binding;

            var penBindingsStack = new StackView();
            for (int i = 0; i < App.Settings?.PenButtons.Count; i++)
            {
                BindingDisplay penBinding;

                var penBindingGroup = new Group
                {
                    Text = $"Pen Button {i + 1}",
                    Orientation = Orientation.Horizontal,
                    ExpandContent = false,
                    Content = penBinding = new BindingDisplay(App.Settings?.PenButtons[i])
                    {
                        MinimumSize = new Size(300, 0),
                        Tag = i
                    }
                };
                penBinding.BindingUpdated += (sender, binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.PenButtons[index] = binding;
                };
                penBindingsStack.AddControl(penBindingGroup);
            }

            var auxBindingsStack = new StackView();
            for (int i = 0; i < App.Settings?.AuxButtons.Count; i++)
            {
                BindingDisplay auxBinding;

                var auxBindingGroup = new Group
                {
                    Text = $"Auxiliary Button {i + 1}",
                    Orientation = Orientation.Horizontal,
                    ExpandContent = false,
                    Content = auxBinding = new BindingDisplay(App.Settings?.AuxButtons[i])
                    {
                        MinimumSize = new Size(300, 0),
                        Tag = i
                    }
                };
                auxBinding.BindingUpdated += (sender, Binding) =>
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    App.Settings.AuxButtons[index] = Binding;
                };
                auxBindingsStack.AddControl(auxBindingGroup);
            }

            var firstRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group("Tip Bindings", tipSettingsStack)
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group("Eraser Bindings", eraserSettingsStack)
                    }
                }
            };

            var secondRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group("Pen Button Bindings", penBindingsStack)
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group("Auxiliary Button Bindings", auxBindingsStack)
                    }
                }
            };

            this.Content = new StackView
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items = 
                {
                    new StackLayoutItem(firstRow, true),
                    new StackLayoutItem(secondRow, true)
                }
            };
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
                    mainButton.Text = Binding?.GetHumanReadableString();
                    BindingUpdated?.Invoke(this, Binding);
                }
                get => this.binding;
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
