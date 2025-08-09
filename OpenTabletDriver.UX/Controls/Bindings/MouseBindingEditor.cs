using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public class MouseBindingEditor : BindingEditor
    {
        public MouseBindingEditor()
        {
            this.Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new Group
                        {
                            Text = "Mouse Buttons",
                            Content = mouseButtons = new MouseBindingDisplayList
                            {
                                Prefix = "Mouse Binding"
                            }
                        },
                        new Group
                        {
                            Text = "Mouse Scrollwheel",
                            Content = new StackLayout
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Spacing = 5,
                                Items =
                                {
                                    new Group
                                    {
                                        Text = "Scroll Up",
                                        Orientation = Orientation.Horizontal,
                                        ExpandContent = false,
                                        Content = scrollUp = new BindingDisplay()
                                    },
                                    new Group
                                    {
                                        Text = "Scroll Down",
                                        Orientation = Orientation.Horizontal,
                                        ExpandContent = false,
                                        Content = scrollDown = new BindingDisplay()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            mouseButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.MouseButtons));
            scrollUp.StoreBinding.Bind(SettingsBinding.Child(c => c.MouseScrollUp));
            scrollDown.StoreBinding.Bind(SettingsBinding.Child(c => c.MouseScrollDown));
        }

        private MouseBindingDisplayList mouseButtons;
        private BindingDisplay scrollUp, scrollDown;

        private class MouseBindingDisplayList : BindingDisplayList
        {
            protected override string GetTextForIndex(int index)
            {
                return index switch
                {
                    0 => "Primary Binding",
                    1 => "Alternate Binding",
                    2 => "Middle Binding",
                    _ => base.GetTextForIndex(index)
                };
            }
        }
    }
}
