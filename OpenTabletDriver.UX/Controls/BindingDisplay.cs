using System;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Binding;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingDisplay : Button
    {
        public BindingDisplay(string binding)
        {
            Binding = BindingReference.FromString(binding);
            
            var bindingCommand = new Command();
            bindingCommand.Executed += (sender, e) =>
            {
                var dialog = new BindingEditorDialog(Binding);
                Binding = dialog.ShowModalAsync(this).ConfigureAwait(false).GetAwaiter().GetResult();
            };
            this.Command = bindingCommand;

            this.MouseDown += (s, e) =>
            {
                if (e.Buttons.HasFlag(MouseButtons.Alternate))
                {
                    var dialog = new AdvancedBindingEditorDialog(Binding);
                    Binding = dialog.ShowModalAsync(this).ConfigureAwait(false).GetAwaiter().GetResult();
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