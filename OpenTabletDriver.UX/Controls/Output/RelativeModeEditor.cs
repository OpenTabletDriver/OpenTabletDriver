using System;
using Eto.Forms;
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

            xSens.ValueBinding.Bind(App.Current.ProfileBinding.Child(p => p.XSensitivity));
            ySens.ValueBinding.Bind(App.Current.ProfileBinding.Child(p => p.YSensitivity));
            rotation.ValueBinding.Bind(App.Current.ProfileBinding.Child(p => p.RelativeRotation));
            resetTime.ValueBinding.Convert<TimeSpan>(
                c => TimeSpan.FromMilliseconds(c),
                v => (float)v.TotalMilliseconds
            ).Bind(App.Current.ProfileBinding.Child(p => p.ResetTime));
        }

        private MaskedTextBox<float> xSens, ySens, rotation, resetTime;
    }
}
