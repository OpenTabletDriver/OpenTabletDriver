using System.Collections.Generic;
using Eto.Drawing;
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
                                            Text = "Tip Settings",
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = "Tip Binding",
                                                        Orientation = Orientation.Horizontal,
                                                        ExpandContent = false,
                                                        Content = tipButton = new BindingDisplay()
                                                    },
                                                    new Group
                                                    {
                                                        Text = "Tip Pressure",
                                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                                        Orientation = Orientation.Horizontal,
                                                        Content = tipPressure = new FloatSlider()
                                                    }
                                                }
                                            }
                                        },
                                        new Group
                                        {
                                            Text = "Eraser Settings",
                                            Content = new StackLayout
                                            {
                                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                                Spacing = 5,
                                                Items =
                                                {
                                                    new Group
                                                    {
                                                        Text = "Eraser Binding",
                                                        ExpandContent = false,
                                                        Orientation = Orientation.Horizontal,
                                                        Content = eraserButton = new BindingDisplay()
                                                    },
                                                    new Group
                                                    {
                                                        Text = "Eraser Pressure",
                                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                                        Orientation = Orientation.Horizontal,
                                                        Content = eraserPressure = new FloatSlider()
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
                            Text = "Pen Buttons",
                            Content = penButtons = new BindingDisplayList
                            {
                                Prefix = "Pen Binding"
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
        }

        private BindingDisplay tipButton, eraserButton;
        private FloatSlider tipPressure, eraserPressure;
        private BindingDisplayList penButtons;
    }
}