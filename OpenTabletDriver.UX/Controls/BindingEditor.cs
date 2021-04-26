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
                                            Prefix = "Pen Button",
                                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                            Spacing = 5
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
                                            Prefix = "Auxiliary Button",
                                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                            Spacing = 5
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipPressure, eraserPressure;
        private BindingDisplayList penButtons, auxButtons;

        public BindableBinding<BindingDisplay, PluginSettingStore> TipButtonStoreBinding => tipButton.StoreBinding;
        public BindableBinding<BindingDisplay, PluginSettingStore> EraserButtonStoreBinding => eraserButton.StoreBinding;

        public BindableBinding<FloatSlider, float> TipPressureValueBinding => tipPressure.ValueBinding;
        public BindableBinding<FloatSlider, float> EraserPressureValueBinding => eraserPressure.ValueBinding;

        public BindableBinding<CustomItemList<PluginSettingStore>, IList<PluginSettingStore>> PenButtonItemSourceBinding => penButtons.ItemSourceBinding;
        public BindableBinding<CustomItemList<PluginSettingStore>, IList<PluginSettingStore>> AuxiliaryButtonItemSourceBinding => auxButtons.ItemSourceBinding;

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
