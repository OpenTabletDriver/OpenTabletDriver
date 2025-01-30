using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class WheelBindingEditor : BindingEditor
    {
        public WheelBindingEditor()
        {
            wheelButtonGroup = new Group
            {
                Text = "Wheel Buttons",
                Content = wheelButtons = new BindingDisplayList
                {
                    Prefix = "Wheel Button Binding"
                }
            };

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
                                        {
                                            Minimum = 1
                                        }
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
                                        {
                                            Minimum = 1
                                        }
                                    }
                                }
                            }
                        },
                        wheelButtonGroup
                    }
                }
            };

            clockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.ClockwiseRotation));
            counterClockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.CounterClockwiseRotation));
            clockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.ClockwiseActivationThreshold));
            counterClockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.CounterClockwiseActivationThreshold));
            wheelButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.WheelButtons));
        }

        private Group wheelButtonGroup;
        private BindingDisplay clockwiseButton, counterClockwiseButton;
        private FloatSlider clockwiseThreshold, counterClockwiseThreshold;
        private BindingDisplayList wheelButtons;

        protected override void OnTabletChanged() 
        {
            base.OnTabletChanged();

            if (Tablet?.Properties?.Specifications?.Wheel != null)
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    wheelButtonGroup.Visible = Tablet.Properties.Specifications.Wheel.Buttons.ButtonCount > 0;
                });
            }
        }
    }
}
