using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class TouchStripBindingEditor : BindingEditor
    {
        public TouchStripBindingEditor()
        {
            this.Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5,
                    Items =
                    {
                        new Group
                        {
                            Text = "Touch Strips",
                            Content = touchStripBindings = new BindingDisplayList
                            {
                                Prefix = "Touch Strip Binding"
                            }
                        }
                    }
                }
            };

            touchStripBindings.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.TouchStrips));
        }

        private BindingDisplayList touchStripBindings;
    }
}
