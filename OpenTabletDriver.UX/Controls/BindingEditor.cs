using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingEditor : Panel
    {
        public BindingEditor()
        {
            Padding = 5;

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
            tipButton.StoreBinding.Bind(SettingsBinding.Child(s => s.TipButton));
            tipPressure.ValueBinding.Bind(SettingsBinding.Child(s => s.TipActivationPressure));

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
            eraserButton.StoreBinding.Bind(SettingsBinding.Child(s => s.EraserButton));
            eraserPressure.ValueBinding.Bind(SettingsBinding.Child(s => s.EraserActivationPressure));

            var penBindings = new BindingDisplayList
            {
                Prefix = "Pen Button",
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };
            penBindings.ItemSourceBinding.Bind(SettingsBinding.Child<IList<PluginSettingStore>>(s => s.PenButtons));

            var auxBindings = new BindingDisplayList
            {
                Prefix = "Auxiliary Buttons",
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };
            auxBindings.ItemSourceBinding.Bind(SettingsBinding.Child<IList<PluginSettingStore>>(s => s.AuxButtons));

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
                        Control = new Group
                        {
                            Text = "Pen Button Bindings",
                            Content = new Scrollable
                            {
                                Border = BorderType.None,
                                Content = penBindings
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Group
                        {
                            Text = "Auxiliary Button Bindings",
                            Content = new Scrollable
                            {
                                Border = BorderType.None,
                                Content = auxBindings
                            }
                        }
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

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipPressure, eraserPressure;

        private Settings settings;
        public Settings Settings
        {
            set
            {
                this.settings = value;
                this.OnSettingsChanged();
            }
            get => this.settings;
        }

        public event EventHandler<EventArgs> SettingsChanged;

        protected virtual void OnSettingsChanged() => SettingsChanged?.Invoke(this, new EventArgs());

        public BindableBinding<BindingEditor, Settings> SettingsBinding
        {
            get
            {
                return new BindableBinding<BindingEditor, Settings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private class BindingDisplayList : CustomItemList<PluginSettingStore>
        {
            public string Prefix { set; get; }

            protected override Control CreateControl(int index, DirectBinding<PluginSettingStore> itemBinding)
            {
                BindingDisplay display;

                var group = new Group
                {
                    Text = $"{Prefix} {index + 1}",
                    Orientation = Orientation.Horizontal,
                    ExpandContent = false,
                    Content = display = new BindingDisplay
                    {
                        MinimumSize = new Size(300, 0)
                    }
                };

                display.StoreBinding.Bind(itemBinding);
                return group;
            }
        }
    }
}
