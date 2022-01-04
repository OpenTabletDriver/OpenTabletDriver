using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class DigitizerSpecificationsEditor : SpecificationsEditor<DigitizerSpecifications>
    {
        public DigitizerSpecificationsEditor()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = enable = new CheckBox
                        {
                            Text = "Enable"
                        }
                    },
                    new Group
                    {
                        Text = "Width (mm)",
                        Orientation = Orientation.Horizontal,
                        Content = width = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Height (mm)",
                        Orientation = Orientation.Horizontal,
                        Content = height = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Horizontal Resolution",
                        Orientation = Orientation.Horizontal,
                        Content = maxX = new FloatNumberBox()
                    },
                    new Group
                    {
                        Text = "Vertical Resolution",
                        Orientation = Orientation.Horizontal,
                        Content = maxY = new FloatNumberBox()
                    }
                }
            };

            enable.CheckedBinding.Cast<bool>().Bind(
                SpecificationsBinding.Convert(
                    c => c != null,
                    v => v ? new DigitizerSpecifications() : null
                )
            );
            enable.CheckedBinding.Bind(width, c => c.Enabled);
            enable.CheckedBinding.Bind(height, c => c.Enabled);
            enable.CheckedBinding.Bind(maxX, c => c.Enabled);
            enable.CheckedBinding.Bind(maxY, c => c.Enabled);

            width.ValueBinding.Bind(SpecificationsBinding.Child<float>(c => c.Width));
            height.ValueBinding.Bind(SpecificationsBinding.Child<float>(c => c.Height));
            maxX.ValueBinding.Bind(SpecificationsBinding.Child<float>(c => c.MaxX));
            maxY.ValueBinding.Bind(SpecificationsBinding.Child<float>(c => c.MaxY));
        }

        private CheckBox enable;
        private MaskedTextBox<float> width, height, maxX, maxY;
    }
}
