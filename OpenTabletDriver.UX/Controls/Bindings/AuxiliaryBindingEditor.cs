using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class AuxiliaryBindingEditor : BindingEditor
    {
        public AuxiliaryBindingEditor()
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
                            Text = "Auxiliary",
                            Content = auxButtons = new BindingDisplayList
                            {
                                Prefix = "Auxiliary Binding"
                            }
                        }
                    }
                }
            };

            auxButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.AuxButtons));
        }

        private BindingDisplayList auxButtons;
    }
}
