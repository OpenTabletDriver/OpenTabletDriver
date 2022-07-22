using Eto.Forms;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class PluginSettingsEditor : DesktopPanel
    {
        private readonly IControlBuilder _controlBuilder;
        private readonly Placeholder _placeholder;

        public PluginSettingsEditor(IControlBuilder controlBuilder)
        {
            _controlBuilder = controlBuilder;

            Content = _placeholder = new Placeholder("No plugin selected.");
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is not SettingsViewModel { Settings: not null, Type: not null } model)
            {
                Content = _placeholder;
                return;
            }

            var enableToggle = new CheckBox
            {
                Text = "Enable"
            };

            enableToggle.CheckedBinding.BindDataContext((SettingsViewModel m) => m.Settings.Enable);

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

            foreach (var control in _controlBuilder.Generate(model.Settings, model.Type))
            {
                var item = new StackLayoutItem(control);
                layout.Items.Add(item);
            }

            Content = layout;
        }
    }
}
