using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListBox<T> : ListBox where T : class
    {
        public T SelectedItem
        {
            set => base.SelectedValue = value;
            get => (T)base.SelectedValue;
        }

        public IList<T> Source
        {
            set => base.DataStore = value;
            get => (IList<T>)base.DataStore;
        }

        public DirectBinding<T> SelectedItemBinding => SelectedValueBinding.Cast<T>();
    }
}