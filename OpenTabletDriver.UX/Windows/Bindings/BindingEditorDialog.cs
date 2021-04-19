using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class BindingEditorDialog : Dialog<PluginSettingStore>
    {
        public BindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Binding Editor";
            Result = currentBinding;

            var inputHandler = new TextArea
            {
                Text = "Press a key or press a mouse button",
                Width = 300,
                Height = 150,
                TextAlignment = TextAlignment.Center,
                ReadOnly = true
            };
            inputHandler.KeyDown += CreateKeyBinding;
            inputHandler.MouseDown += CreateMouseBinding;

            var clearCommand = new Command { MenuText = "Clear" };
            clearCommand.Executed += ClearBinding;
            var clearButton = new Button
            {
                Text = "Clear",
                Command = clearCommand,
            };
            clearButton.KeyDown += CreateKeyBinding;

            this.Content = new TableLayout
            {
                Rows = 
                {
                    inputHandler,
                    clearButton
                },
                Padding = new Padding(5),
                Spacing = new Size(5, 5)
            };
        }

        private void CreateKeyBinding(object sender, KeyEventArgs e)
        {
            var store = new PluginSettingStore(typeof(KeyBinding));
            store[nameof(KeyBinding.Keys)].SetValue(e.Key.ToString());
            Close(store);
        }

        private void CreateMouseBinding(object sender, MouseEventArgs e)
        {
            var store = new PluginSettingStore(typeof(MouseBinding));
            store[nameof(MouseBinding.Button)].SetValue(ParseMouseButton(e));
            Close(store);
        }

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(null);
        }

        private static string ParseMouseButton(MouseEventArgs e)
        {
            switch (e.Buttons)
            {
                case MouseButtons.Primary:
                    return nameof(MouseButton.Left);
                case MouseButtons.Middle:
                    return nameof(MouseButton.Middle);
                case MouseButtons.Alternate:
                    return nameof(MouseButton.Right);
                default:
                    return nameof(MouseButton.None);
            }
        }
    }
}
