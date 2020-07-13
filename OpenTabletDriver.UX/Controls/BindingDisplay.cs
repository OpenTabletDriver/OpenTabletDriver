using System;
using Eto.Forms;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingDisplay : Button, IViewModelRoot<BindingViewModel>
    {
        public BindingDisplay()
        {
            this.DataContext = new BindingViewModel();
            this.TextBinding.BindDataContext((BindingViewModel m) => m.Binding);
            
            var bindingCommand = new Command();
            bindingCommand.BindDataContext(c => c.MenuText, (BindingViewModel m) => m.Binding);
            bindingCommand.Executed += async (sender, e) =>
            {
                var dialog = new BindingEditorDialog(ViewModel.Binding);
                ViewModel.Binding = await dialog.ShowModalAsync(this);
            };
            this.Command = bindingCommand;

            ViewModel.BindingUpdated += (s, e) => BindingUpdated?.Invoke(this, e);

            this.MouseDown += async (s, e) => 
            {
                if (e.Buttons.HasFlag(MouseButtons.Alternate))
                {
                    var dialog = new AdvancedBindingEditorDialog(ViewModel.Binding);
                    ViewModel.Binding = await dialog.ShowModalAsync(this);
                }
            };
        }

        public BindingDisplay(string binding) : this()
        {
            ViewModel.Binding = binding;
        }

        public event EventHandler<string> BindingUpdated;

        public BindingViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (BindingViewModel)this.DataContext;
        }
    }
}