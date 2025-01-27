using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class WheelBindingEditor : BindingEditor
    {
        public WheelBindingEditor()
        {
            this.Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5,
                    Items =
                    {
                        new Group
                        {
                            Text = "Clockwise Rotation Settings",
                            Content = new StackLayout
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Spacing = 5,
                                Items =
                                {
                                    new Group
                                    {
                                        Text = "Clockwise Rotation",
                                        Orientation = Orientation.Horizontal,
                                        ExpandContent = false,
                                        Content = clockwiseButton = new BindingDisplay()
                                    },
                                    new Group
                                    {
                                        Text = "Clockwise Rotation Threshold",
                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                        Orientation = Orientation.Horizontal,
                                        Content = clockwiseThreshold = new FloatSlider()
                                    }
                                }
                            }
                        },
                        new Group
                        {
                            Text = "Counter-Clockwise Rotation Settings",
                            Content = new StackLayout
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Spacing = 5,
                                Items =
                                {
                                    new Group
                                    {
                                        Text = "Counter-Clockwise Rotation",
                                        ExpandContent = false,
                                        Orientation = Orientation.Horizontal,
                                        Content = counterClockwiseButton = new BindingDisplay()
                                    },
                                    new Group
                                    {
                                        Text = "Counter-Clockwise Rotation Threshold",
                                        ToolTip = "The minimum threshold in order for the assigned binding to activate.",
                                        Orientation = Orientation.Horizontal,
                                        Content = counterClockwiseThreshold = new FloatSlider()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            clockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.ClockwiseRotation));
            counterClockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.CounterClockwiseRotation));
            clockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.ClockwiseActivationThreshold));
            counterClockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.CounterClockwiseActivationThreshold));
        }

        private BindingDisplay clockwiseButton, counterClockwiseButton;
        private FloatSlider clockwiseThreshold, counterClockwiseThreshold;
    }
}
