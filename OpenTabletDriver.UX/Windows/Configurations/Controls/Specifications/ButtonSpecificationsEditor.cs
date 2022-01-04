using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Text;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class ButtonSpecificationsEditor : SpecificationsEditor<ButtonSpecifications>
    {
        public ButtonSpecificationsEditor()
        {
            this.Content = new StackLayout
            {
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = enable = new CheckBox
                        {
                            Text = "Enable",
                        }
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
                    v => v ? new ButtonSpecifications() : null
                )
            );

            enable.CheckedBinding.Bind(buttonCount, b => b.Enabled);

            buttonCount.ValueBinding.Bind(SpecificationsBinding.Child(b => b.ButtonCount));
        }

        private CheckBox enable;
        private MaskedTextBox<uint> buttonCount;
    }
}
