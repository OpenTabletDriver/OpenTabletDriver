using System;
using System.Collections.Generic;
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

            xSens.ValueBinding.BindDataContext<App>(a => a.Settings.XSensitivity);
            ySens.ValueBinding.BindDataContext<App>(a => a.Settings.YSensitivity);
            rotation.ValueBinding.BindDataContext<App>(a => a.Settings.RelativeRotation);
            resetTime.ValueBinding.Convert<TimeSpan>(
                c => TimeSpan.FromMilliseconds(c),
                v => (float)v.TotalMilliseconds
            ).BindDataContext<App>(a => a.Settings.ResetTime);
        }

        private MaskedTextBox<float> xSens, ySens, rotation, resetTime;
    }
}
