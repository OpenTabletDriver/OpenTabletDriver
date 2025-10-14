using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;
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
                Content = body = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5
                }
            };
        }

        private StackLayout body;
        private Group[] wheelsButtons = Array.Empty<Group>();

        private void BuildWheelGroup(TabletReference tablet, int index)
        {
            Group wheelButtonGroup;
            BindingDisplay clockwiseButton, counterClockwiseButton;
            FloatSlider clockwiseThreshold, counterClockwiseThreshold;
            BindingDisplayList wheelButtons;

            wheelButtonGroup = new Group
            {
                Text = "Wheel Buttons",
                Content = wheelButtons = new BindingDisplayList
                {
                    Prefix = "Wheel Button Binding"
                }
            };

            var layout = new StackLayout
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
            };

            clockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.Wheels[index].ClockwiseRotation));
            counterClockwiseButton.StoreBinding.Bind(SettingsBinding.Child(c => c.Wheels[index].CounterClockwiseRotation));
            clockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.Wheels[index].ClockwiseActivationThreshold));
            counterClockwiseThreshold.ValueBinding.Bind(SettingsBinding.Child(c => c.Wheels[index].CounterClockwiseActivationThreshold));
            wheelButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.Wheels[index].WheelButtons));

            //wheelButtonGroup.Visible = tablet.Properties.Specifications.Wheels[index].Buttons.ButtonCount > 0;
            wheelsButtons[index] = wheelButtonGroup;

            body.Items.Add(
                /* new Group
                {
                    Text = $"Wheel {index + 1}",
                    Content = layout
                } */
                layout
            );
        }

        protected override void OnProfileChanged() => Application.Instance.AsyncInvoke(async () =>
        {
            base.OnProfileChanged();

            if (Profile == null) return;

            var tablet = Profile != null ? await Profile.GetTabletReference() : null;
            if (tablet?.Properties?.Specifications?.Wheels == null) return;

            body.Items.Clear();

            var wheelsCount = Profile.BindingSettings.Wheels.Count;
            wheelsButtons = new Group[wheelsCount];

            for (int i = 0; i < wheelsCount; i++)
                BuildWheelGroup(null, i);
        });

        protected override void OnTabletChanged()
        {
            base.OnTabletChanged();

            if (Tablet?.Properties?.Specifications?.Wheels != null)
            {
                Application.Instance.AsyncInvoke(() =>
                {
                    for (var i = 0; i < Tablet.Properties.Specifications.Wheels.Count; i++)
                    {
                        if (i >= wheelsButtons.Length || wheelsButtons[i] == null) return;
                        var wheelSpecification = Tablet.Properties.Specifications.Wheels[i];
                        wheelsButtons[i].Visible = wheelSpecification.Buttons.ButtonCount > 0;
                    }
                });
            }
        }
    }
}
