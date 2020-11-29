using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingDisplay : Button
    {
        public BindingDisplay(string binding)
        {
            Binding = BindingReference.FromString(binding);
            
            var bindingCommand = new Command();
            bindingCommand.Executed += async (sender, e) =>
            {
                var dialog = new BindingEditorDialog(Binding);
                Binding = await dialog.ShowModalAsync(this);
            };
            this.Command = bindingCommand;

            this.MouseDown += async (s, e) => 
            {
                if (e.Buttons.HasFlag(MouseButtons.Alternate))
                {
                    var dialog = new AdvancedBindingEditorDialog(Binding);
                    Binding = await dialog.ShowModalAsync(this);
                }
            };
        }

        public event EventHandler<BindingReference> BindingUpdated;

        private BindingReference _binding;
        public BindingReference Binding
        {
            set
            {
                _binding = value;
                Text = Binding.ToDisplayString();
                BindingUpdated?.Invoke(this, Binding);
            }
            get => _binding ?? BindingReference.None;
        }
    }
}
