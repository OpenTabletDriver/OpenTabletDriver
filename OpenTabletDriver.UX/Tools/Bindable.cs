using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Tools
{
    public abstract class Bindable : IBindable
    {
        private object dataContext;

        public object DataContext
        {
            get => dataContext;
            set
            {
                dataContext = value;
                DataContextChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BindingCollection Bindings { get; } = new BindingCollection();

        public event EventHandler<EventArgs> DataContextChanged;
    }
}