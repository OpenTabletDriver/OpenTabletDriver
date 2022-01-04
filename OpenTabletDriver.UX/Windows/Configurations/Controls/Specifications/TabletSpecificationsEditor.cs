using System;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public class TabletSpecificationsEditor : SpecificationsEditor<TabletSpecifications>
    {
        public TabletSpecificationsEditor()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                Items =
                {
                    new Expander
                    {
                        Header = "Digitizer",
                        Content = digitizer = new DigitizerSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Pen",
                        Content = pen = new PenSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Auxiliary Buttons",
                        Padding = 5,
                        Content = auxButtons = new ButtonSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Touch",
                        Content = touch = new DigitizerSpecificationsEditor()
                    },
                    new Expander
                    {
                        Header = "Mouse",
                        Padding = 5,
                        Content = mouseButtons = new ButtonSpecificationsEditor()
                    }
                }
            };

            digitizer.SpecificationsBinding.Bind(SpecificationsBinding.Child(c => c.Digitizer));
            pen.SpecificationsBinding.Bind(SpecificationsBinding.Child(c => c.Pen));
            auxButtons.SpecificationsBinding.Bind(SpecificationsBinding.Child(c => c.AuxiliaryButtons));
            touch.SpecificationsBinding.Bind(SpecificationsBinding.Child(c => c.Touch));
            mouseButtons.SpecificationsBinding.Bind(SpecificationsBinding.Child(c => c.MouseButtons));
        }

        private DigitizerSpecificationsEditor digitizer, touch;
        private PenSpecificationsEditor pen;
        private ButtonSpecificationsEditor auxButtons;
        private ButtonSpecificationsEditor mouseButtons;
    }
}
