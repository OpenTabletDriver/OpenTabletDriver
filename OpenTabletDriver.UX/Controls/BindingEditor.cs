using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
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
            this.DataContext = App.Settings;

            BindingDisplay tipButton, eraserButton;
            FloatSlider tipPressure, eraserPressure;

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
                    new Group
                    {
                        Text = "Tip Pressure",
                        Orientation = Orientation.Horizontal,
                        Content = tipPressure = new FloatSlider()
                    }
                }
            };
            tipButton.BindingUpdated += (sender, binding) => App.Settings.TipButton = binding;
            tipPressure.ValueBinding.BindDataContext<Settings>(s => s.TipActivationPressure);

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
                    new Group
                    {
                        Text = "Eraser Pressure",
                        Orientation = Orientation.Horizontal,
                        Content = eraserPressure = new FloatSlider()
                    }
                }
            };
            eraserButton.BindingUpdated += (sender, binding) => App.Settings.EraserButton = binding;
            eraserPressure.ValueBinding.BindDataContext<Settings>(s => s.EraserActivationPressure);

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
    }
}
