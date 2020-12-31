using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
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
                        Text = "Preview",
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
                "Click a button to set a key or mouse binding",
                "Right clicking opens the advanced binding editor, which allows you to use plugin bindings.",
                new PaddingSpacerItem(),
            };
        }

        private class DemoBindingDisplay : BindingEditor.BindingDisplay
        {
            public DemoBindingDisplay()
                : base(PluginSettingStore.FromPath(typeof(OpenTabletDriver.Plugin.IBinding).FullName))
            {
                Width = 256;
            }
        }
    }
}