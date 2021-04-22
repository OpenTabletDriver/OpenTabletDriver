using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.UX.Controls.Generic;

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

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipPressure, eraserPressure;

        public void UpdateBindings()
        {
            this.DataContext = App.Settings;

            var tipSettingsStack = new StackView
            {
                Items =
                {
                    new Group
                    {
                        Text = "Tip Button",
                        Orientation = Orientation.Horizontal,
                        ExpandContent = false,
                        Content = tipButton = new BindingDisplay
                        {
                            MinimumSize = new Size(300, 0),
                            Store = App.Settings?.TipButton
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
            tipButton.StoreBinding.BindDataContext<Settings>(s => s.TipButton);
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
                        Content = eraserButton = new BindingDisplay
                        {
                            MinimumSize = new Size(300, 0),
                            Store = App.Settings?.EraserButton
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
            eraserButton.StoreBinding.BindDataContext<Settings>(s => s.EraserButton);
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
                    Content = penBinding = new BindingDisplay
                    {
                        MinimumSize = new Size(300, 0),
                        Tag = i,
                        Store = App.Settings?.PenButtons[i]
                    }
                };
                penBinding.StoreChanged += (sender, e) =>
                {
                    var display = sender as BindingDisplay;
                    var index = (int)display.Tag;
                    App.Settings.PenButtons[index] = display.Store;
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
                    Content = auxBinding = new BindingDisplay
                    {
                        MinimumSize = new Size(300, 0),
                        Tag = i,
                        Store = App.Settings?.AuxButtons[i]
                    }
                };
                auxBinding.StoreChanged += (sender, e) =>
                {
                    var display = sender as BindingDisplay;
                    var index = (int)display.Tag;
                    App.Settings.AuxButtons[index] = display.Store;
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
    }
}
