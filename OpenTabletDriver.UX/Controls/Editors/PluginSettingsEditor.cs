using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class PluginSettingsEditor : DesktopPanel
    {
        public PluginSettingsEditor(IControlBuilder controlBuilder, PluginSettings settings, Type type)
        {
            DataContext = settings;

            var enableToggle = new CheckBox
            {
                Text = "Enable"
            };

            enableToggle.CheckedBinding.BindDataContext((PluginSettings s) => s.Enable);

            var layout = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    enableToggle
                }
            };

            foreach (var control in controlBuilder.Generate(settings, type))
            {
                var item = new StackLayoutItem(control);
                layout.Items.Add(item);
            }

            Content = layout;
        }
    }
}
