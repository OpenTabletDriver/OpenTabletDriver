using System;
using TabletDriverPlugin;

namespace OpenTabletDriver.UX.Controls
{
    public class BindingViewModel : ViewModelBase
    {
        private string _binding;
        public string Binding
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _binding, value);
                BindingUpdated?.Invoke(this, Binding);
            }
            get => _binding;
        }

        public event EventHandler<string> BindingUpdated;
    }
}