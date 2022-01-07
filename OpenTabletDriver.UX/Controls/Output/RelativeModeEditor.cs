using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;
using OpenTabletDriver.UX.Controls.Output.Area;

namespace OpenTabletDriver.UX.Controls.Output
{
    public class RelativeModeEditor : Panel
    {
        public RelativeModeEditor()
        {
            this.Content = new Group
            {
                Text = "Relative",
                Content = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Spacing = 5,
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new UnitGroup
                        {
                            Text = "X Sensitivity",
                            Orientation = Orientation.Horizontal,
                            Unit = "px/mm",
                            Content = xSens = new FloatNumberBox()
                        },
                        new UnitGroup
                        {
                            Text = "Y Sensitivity",
                            Orientation = Orientation.Horizontal,
                            Unit = "px/mm",
                            Content = ySens = new FloatNumberBox()
                        },
                        new UnitGroup
                        {
                            Text = "Rotation",
                            Orientation = Orientation.Horizontal,
                            Unit = "°",
                            Content = rotation = new FloatNumberBox()
                        },
                        new UnitGroup
                        {
                            Text = "Reset Time",
                            Orientation = Orientation.Horizontal,
                            Unit = "ms",
                            Content = resetTime = new FloatNumberBox()
                        },
                        new StackLayoutItem(null, true)
                    }
                }
            };

            xSens.ValueBinding.Bind(SettingsBinding.Child(s => s.XSensitivity));
            ySens.ValueBinding.Bind(SettingsBinding.Child(s => s.YSensitivity));
            rotation.ValueBinding.Bind(SettingsBinding.Child(s => s.RelativeRotation));
            resetTime.ValueBinding.Convert<TimeSpan>(
                c => TimeSpan.FromMilliseconds(c),
                v => (float)v.TotalMilliseconds
            ).Bind(SettingsBinding.Child(s => s.ResetTime));
        }

        private MaskedTextBox<float> xSens, ySens, rotation, resetTime;

        private RelativeModeSettings settings;
        public RelativeModeSettings Settings
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

        public BindableBinding<RelativeModeEditor, RelativeModeSettings> SettingsBinding
        {
            get
            {
                return new BindableBinding<RelativeModeEditor, RelativeModeSettings>(
                    this,
                    c => c.Settings,
                    (c, v) => c.Settings = v,
                    (c, h) => c.SettingsChanged += h,
                    (c, h) => c.SettingsChanged -= h
                );
            }
        }
    }
}
