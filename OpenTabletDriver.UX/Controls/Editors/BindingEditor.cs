using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Dialogs;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class BindingEditor : DesktopPanel
    {
        private readonly App _app;
        private readonly DirectBinding<PluginSettings?> _binding;

        public BindingEditor(App app, IPluginFactory pluginFactory, DirectBinding<PluginSettings?> binding)
        {
            _app = app;
            _binding = binding;

            var button = new Button(ShowBindingDialog);
            button.TextBinding.Bind(binding.Convert(s => s == null ? string.Empty : s.Format(pluginFactory)));

            var advancedButton = new Button(ShowAdvancedBindingDialog)
            {
                Text = "..."
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(button, true),
                    advancedButton
                }
            };
        }

        private void ShowBindingDialog(object? sender, EventArgs e)
        {
            var dialog = _app.ShowDialog<BindingDialog>(ParentWindow, _binding);
            if (dialog.Result?.Result == DialogResult.Ok)
                _binding.DataValue = dialog.Result.Model;
        }

        private void ShowAdvancedBindingDialog(object? sender, EventArgs e)
        {
            var dialog = _app.ShowDialog<AdvancedBindingDialog>(ParentWindow, _binding);
            if (dialog.Result?.Result == DialogResult.Ok)
                _binding.DataValue = dialog.Result.Model;
        }
    }
}
