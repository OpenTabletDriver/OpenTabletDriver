using System;
using Eto.Forms;
using OpenTabletDriverUX.Windows;
using TabletDriverLib.Plugins;
using IBinding = TabletDriverPlugin.IBinding;

namespace OpenTabletDriverUX.Controls
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
                var dialog = new BindingEditorDialog();
                dialog.Result = ViewModel.Binding;
                var result = await dialog.ShowModalAsync(this);
                ViewModel.Binding = result;
            };
            this.Command = bindingCommand;

            ViewModel.BindingUpdated += (s, e) => BindingUpdated?.Invoke(this, e);
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