using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.UX.Windows
{
    public class BindingEditorDialog : Dialog<BindingReference>
    {
        public BindingEditorDialog(BindingReference currentBinding = null)
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
            Return(BindingReference.None);
        }

        private void Return<T>(T binding) where T : OpenTabletDriver.Plugin.IBinding
        {
            var str = BindingTools.GetBindingString(binding);
            var bindRef = BindingReference.FromString(str);
            Return(bindRef);
        }

        private void Return(BindingReference result)
        {
            Close(result);
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
