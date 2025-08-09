using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Bindings")]
    public class BindingPage : StylizedPage
    {
        public BindingPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new StackLayoutItem
                {
                    Expand = true,
                    Control = new Group
                    {
                        Text = "Demo",
                        Content = new StackedContent
                        {
                            new PaddingSpacerItem(),
                            new StackLayoutItem
                            {
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Control = new Group
                                {
                                    Text = "Demo Binding",
                                    Orientation = Orientation.Horizontal,
                                    Content = new StackLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Items =
                                        {
                                            new Panel
                                            {
                                                Width = 50
                                            },
                                            new DemoBindingDisplay
                                            {
                                                Width = 250
                                            }
                                        }
                                    }
                                }
                            },
                            new PaddingSpacerItem()
                        }
                    }
                },
                new StylizedText("This is the binding editor.", SystemFonts.Bold(9), new Padding(0, 0, 0, 4)),
                "It allows you to set specific actions that OpenTabletDriver will perform when, for example, a tablet button is pressed.",
                "Click on the left button to capture a mouse or keyboard binding.",
                "Click on the right button to open the advanced binding editor, which allows you to use plugin bindings.",
                new PaddingSpacerItem(),
            };
        }

        private class DemoBindingDisplay : BindingDisplay
        {
            public DemoBindingDisplay()
            {
                Width = 512;
            }
        }
    }
}
