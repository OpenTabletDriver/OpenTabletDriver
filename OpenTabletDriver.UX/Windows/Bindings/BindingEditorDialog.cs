using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class BindingEditorDialog : Dialog<PluginSettingStore>
    {
        public BindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Binding Editor";
            Result = currentBinding;

            var inputHandler = new Label
            {
                Text = "Press a key or press a mouse button",
                TextAlignment = TextAlignment.Center
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

            this.Content = new StackLayout
            {
                Width = 300,
                Height = 200,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(inputHandler, true),
                    clearButton
                }
            };
        }

        private void CreateKeyBinding(object sender, KeyEventArgs e)
        {
            var keyBind = new KeyBinding
            {
                Property = e.Key.ToString(),
            };
            Return(keyBind);
        }

        private void CreateMouseBinding(object sender, MouseEventArgs e)
        {
            var mouseBind = new MouseBinding
            {
                Property = ParseMouseButton(e)
            };
            Return(mouseBind);
        }

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(null);
        }

        private void Return<T>(T binding) where T : OpenTabletDriver.Plugin.IBinding
        {
            Close(new PluginSettingStore(binding));
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
