using System.Diagnostics;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class WheelBindingEditor : BindingEditor
    {
        public WheelBindingEditor(int wheelIndex)
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
                                    new UnitGroup
                                    {
                                        Text = "Clockwise Rotation Threshold",
                                        ToolTip = "The minimum threshold in degrees in order for the assigned binding to activate.",
                                        Orientation = Orientation.Horizontal,
                                        Content = clockwiseThreshold = new FloatSlider()
                                        {
                                            Minimum = 1,
                                            Maximum = 360,
                                            SnapToTick = true,
                                        },
                                        Unit = "°"
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
                                    new UnitGroup
                                    {
                                        Text = "Counter-Clockwise Rotation Threshold",
                                        ToolTip = "The minimum threshold in degrees in order for the assigned binding to activate.",
                                        Orientation = Orientation.Horizontal,
                                        Content = counterClockwiseThreshold = new FloatSlider()
                                        {
                                            Minimum = 1,
                                            Maximum = 360,
                                            SnapToTick = true,
                                        },
                                        Unit = "°"
                                    }
                                }
                            }
                        },
                        wheelButtonGroup
                    }
                }
            };

            SettingsBinding.DataValueChanged += (sender, args) =>
            {
                if (sender is not DelegateBinding<BindingSettings> delegateBinding || delegateBinding.DataValue == null) return;

                var wheel = delegateBinding.DataValue.WheelBindings.Count > 0
                    ? delegateBinding.DataValue.WheelBindings[wheelIndex]
                    : null;

                if (wheel != null)
                {
                    // we manually handle Minimum and StepSize, as binding it to the model interferes with model updates
                    // e.g. switching from a tablet with a Minimum/StepSize of 15 to a tablet with StepSize 5 would
                    //   cause the 2nd tablet to have its values increased to the minimum if they were below this
                    clockwiseThreshold.Minimum = clockwiseThreshold.StepSize =
                        counterClockwiseThreshold.Minimum = counterClockwiseThreshold.StepSize = 1;

                    this.DataContext = wheel;

                    Debug.Assert(wheel.StepSize.HasValue); // should've been set at some point by the daemon

                    // manually handle Minimum and StepSize part 2 of 2
                    clockwiseThreshold.Minimum = clockwiseThreshold.StepSize =
                        counterClockwiseThreshold.Minimum = counterClockwiseThreshold.StepSize = (int)wheel.StepSize.Value;
                }
            };

            // this is Minimum and StepSize handling if attached via the model
            // but causes model values to change before the new StepSize can be bound
            //
            //var minimumBinding = Binding.Property((WheelBindingSettings wbs) => wbs.StepSize).Convert(x => x ?? 1);
            //clockwiseThreshold.BindDataContext(x => x.StepSize, minimumBinding, DualBindingMode.OneWay);
            //counterClockwiseThreshold.BindDataContext(x => x.StepSize, minimumBinding, DualBindingMode.OneWay);
            //clockwiseThreshold.BindDataContext(x => x.Minimum, minimumBinding, DualBindingMode.OneWay);
            //counterClockwiseThreshold.BindDataContext(x => x.Minimum, minimumBinding, DualBindingMode.OneWay);

            clockwiseButton.StoreBinding.BindDataContext((WheelBindingSettings wbs) =>
                wbs.ClockwiseRotation);
            counterClockwiseButton.StoreBinding.BindDataContext((WheelBindingSettings wbs) =>
                wbs.CounterClockwiseRotation);
            clockwiseThreshold.ValueBinding.BindDataContext((WheelBindingSettings wbs) =>
                wbs.ClockwiseActivationThreshold);
            counterClockwiseThreshold.ValueBinding.BindDataContext((WheelBindingSettings wbs) =>
                wbs.CounterClockwiseActivationThreshold);

            wheelButtons.ItemSourceBinding.BindDataContext((WheelBindingSettings wbs) => wbs.WheelButtons);

            // set wheel button group visible based on whether there are wheel buttons to assign
            wheelButtonGroup.BindDataContext(x => x.Visible,
                Binding.Property((WheelBindingSettings wbs) => wbs.WheelButtons).Convert(x => x is { Count: > 0 }));
        }

        private Group wheelButtonGroup;
        private BindingDisplay clockwiseButton, counterClockwiseButton;
        private FloatSlider clockwiseThreshold, counterClockwiseThreshold;
        private BindingDisplayList wheelButtons;
    }
}
