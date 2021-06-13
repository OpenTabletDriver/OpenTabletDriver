using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingEditor : Panel
    {
        public BindingEditor()
        {
            this.Content = new TableLayout
            {
                Padding = 5,
                Rows = 
                {
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = new Group
                                {
                                    Text = "Tip Bindings",
                                    Content = new StackView
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
                                    }
                                }
                            },
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = new Group
                                {
                                    Text = "Eraser Bindings",
                                    Content = new StackView
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
                                    }
                                }
                            }
                        }
                    },
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = new Group
                                {
                                    Text = "Pen Button Bindings",
                                    Content = new Scrollable
                                    {
                                        Border = BorderType.None,
                                        Content = penButtons = new BindingDisplayList
                                        {
                                            Prefix = "Pen Button"
                                        }
                                    }
                                }
                            },
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = new Group
                                {
                                    Text = "Auxiliary Button Bindings",
                                    Content = new Scrollable
                                    {
                                        Border = BorderType.None,
                                        Content = auxButtons = new BindingDisplayList
                                        {
                                            Prefix = "Auxiliary Button"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            tipButton.StoreBinding.Bind(SettingsBinding.Child(c => c.TipButton));
            eraserButton.StoreBinding.Bind(SettingsBinding.Child(c => c.EraserButton));
            tipPressure.ValueBinding.Bind(SettingsBinding.Child(c => c.TipActivationPressure));
            eraserPressure.ValueBinding.Bind(SettingsBinding.Child(c => c.EraserActivationPressure));
            penButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.PenButtons));
            auxButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.AuxButtons));
        }

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipPressure, eraserPressure;
        private BindingDisplayList penButtons, auxButtons;
        
        private BindingSettings settings;
        public BindingSettings Settings
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
        
        public BindableBinding<BindingEditor, BindingSettings> SettingsBinding
        {
            get
            {
                return new BindableBinding<BindingEditor, BindingSettings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }

        private class BindingDisplayList : GeneratedItemList<PluginSettingStore>
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
