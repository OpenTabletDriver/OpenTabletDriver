using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class PenBindingEditor : BindingEditor
    {
        public PenBindingEditor()
        {
            this.Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new TableLayout
                        {
                            Rows =
                            {
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Group
                                        {
                                            Text = Strings.TipSettings,
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = Strings.TipBinding,
                                                        Orientation = Orientation.Horizontal,
                                                        ExpandContent = false,
                                                        Content = tipButton = new BindingDisplay()
                                                    },
                                                    new UnitGroup
                                                    {
                                                        Text = Strings.TipThreshold,
                                                        ToolTip = Strings.Theminimumthresholdinorderfortheassignedbindingtoactivate,
                                                        Orientation = Orientation.Horizontal,
                                                        Content = tipThreshold = new FloatSlider(),
                                                        Unit = "%"
                                                    }
                                                }
                                            }
                                        },
                                        new Group
                                        {
                                            Text = Strings.EraserSettings,
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = Strings.EraserBinding,
                                                        ExpandContent = false,
                                                        Orientation = Orientation.Horizontal,
                                                        Content = eraserButton = new BindingDisplay()
                                                    },
                                                    new UnitGroup
                                                    {
                                                        Text = Strings.EraserThreshold,
                                                        ToolTip = Strings.Theminimumthresholdinorderfortheassignedbindingtoactivate,
                                                        Orientation = Orientation.Horizontal,
                                                        Content = eraserThreshold = new FloatSlider(),
                                                        Unit = "%"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Group
                        {
                            Text = Strings.PenButtons,
                            Content = penButtons = new BindingDisplayList
                            {
                                Prefix = "Pen Binding"
                            }
                        },
                        new Group {
                            Text = Strings.Miscellaneous,
                            Content = new StackLayout {
                                Orientation = Orientation.Horizontal,
                                Items = {
                                    new Group {
                                        Orientation = Orientation.Horizontal,
                                        ToolTip = Strings.Disablepressureifitisavailable,
                                        Content = disablePressure = new CheckBox {
                                            Text = Strings.DisablePressure,
                                        }
                                    },
                                    new Group {
                                        Orientation = Orientation.Horizontal,
                                        ToolTip = Strings.Disabletiltifitisavailable,
                                        Content = disableTilt = new CheckBox {
                                            Text = Strings.DisableTilt,
                                        }
                                    },
                                    new Group {
                                        Orientation = Orientation.Horizontal,
                                        ToolTip = Strings.PenBindingsrequirepressuretoactivate,
                                        Content = enableDragBindings = new CheckBox {
                                            Text = Strings.DragBindings,
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            };

            tipButton.StoreBinding.Bind(SettingsBinding.Child(c => c.TipButton));
            eraserButton.StoreBinding.Bind(SettingsBinding.Child(c => c.EraserButton));
            tipThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.TipActivationThreshold));
            eraserThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.EraserActivationThreshold));
            penButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.PenButtons)!);
            disablePressure.CheckedBinding.Cast<bool>().Bind(SettingsBinding.Child(c => c.DisablePressure));
            disableTilt.CheckedBinding.Cast<bool>().Bind(SettingsBinding.Child(c => c.DisableTilt));
            enableDragBindings.CheckedBinding.Cast<bool>().Bind(SettingsBinding.Child(c => c.EnableDragBindings));
        }

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipThreshold, eraserThreshold;
        private CheckBox disablePressure, disableTilt, enableDragBindings;
        private BindingDisplayList penButtons;
    }
}
