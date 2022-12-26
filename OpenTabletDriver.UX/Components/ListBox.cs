using Eto.Forms;
using JetBrains.Annotations;

namespace OpenTabletDriver.UX.Components
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
