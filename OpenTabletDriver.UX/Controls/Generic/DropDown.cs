using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DropDown<T> : DropDown where T : class
    {
        public T SelectedItem
        {
            set => this.SelectedValue = value;
            get => this.SelectedValue as T;
        }

        public BindableBinding<DropDown<T>, T> SelectedItemBinding
        {
            get
            {
                return new BindableBinding<DropDown<T>, T>(
                    this,
                    c => c.SelectedValue as T,
                    (c, v) => c.SelectedValue = v,
                    (c, h) => c.SelectedValueChanged += h,
                    (c, h) => c.SelectedValueChanged -= h
                );
            }
        }
    }
}
