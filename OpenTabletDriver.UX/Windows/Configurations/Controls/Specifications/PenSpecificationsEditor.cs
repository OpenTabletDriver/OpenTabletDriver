using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class PenSpecificationsEditor : SpecificationsEditor<PenSpecifications>
    {
        public PenSpecificationsEditor()
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
                        Text = "Max Pressure",
                        Orientation = Orientation.Horizontal,
                        Content = maxPressure = new UnsignedIntegerNumberBox()
                    },
                    new Group
                    {
                        Text = "Button Count",
                        Orientation = Orientation.Horizontal,
                        Content = buttonCount = new UnsignedIntegerNumberBox()
                    }
                }
            };

            enable.CheckedBinding.Cast<bool>().Bind(
                SpecificationsBinding.Convert(
                    c => c != null,
                    v => v ? new PenSpecifications() : null
                )
            );
            enable.CheckedBinding.Bind(maxPressure, c => c.Enabled);
            enable.CheckedBinding.Bind(buttonCount, c => c.Enabled);

            maxPressure.ValueBinding.Bind(SpecificationsBinding.Child(c => c.MaxPressure));
            buttonCount.ValueBinding.Bind(SpecificationsBinding.Child(c => c.Buttons.ButtonCount));
        }

        private CheckBox enable;
        private MaskedTextBox<uint> maxPressure;
        private UnsignedIntegerNumberBox buttonCount;
    }
}
