using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls.Specifications
{
    public abstract class SpecificationsEditor<T> : Panel where T : class
    {
        private T specification;
        public T Specifications
        {
            set
            {
                this.specification = value;
                this.OnSpecificationsChanged();
            }
            get => this.specification;
        }

        public event EventHandler<EventArgs> SpecificationsChanged;

        protected virtual void OnSpecificationsChanged() => SpecificationsChanged?.Invoke(this, new EventArgs());

        public BindableBinding<SpecificationsEditor<T>, T> SpecificationsBinding
        {
            get
            {
                return new BindableBinding<SpecificationsEditor<T>, T>(
                    this,
                    c => c.Specifications,
                    (c, v) => c.Specifications = v,
                    (c, h) => c.SpecificationsChanged += h,
                    (c, h) => c.SpecificationsChanged -= h
                );
            }
        }
    }
}
