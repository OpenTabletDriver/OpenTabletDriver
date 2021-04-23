using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListBox<T> : ListBox where T : class
    {
        public event EventHandler<EventArgs> SelectedItemChanged;

        public T SelectedItem
        {
            set => base.SelectedValue = value;
            get => (T)base.SelectedValue;
        }

        public IList<T> Source
        {
            set => base.DataStore = value;
            get => base.DataStore as IList<T>;
        }

        protected virtual void OnSelectedItemChanged(EventArgs e) => SelectedItemChanged?.Invoke(this, e);

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            this.OnSelectedItemChanged(e);
        }
    }
}