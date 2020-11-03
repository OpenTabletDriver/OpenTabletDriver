using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListEditor : CollectionEditor<IList<string>>
    {
        public ListEditor(
            string name,
            Func<IList<string>> getValue,
            Action<IList<string>> setValue
        ) : base(name, getValue, setValue)
        {
            // Workaround for weird layout bug that only affects this control
            (base.Content as StackLayout).Spacing = 0;
        }

        protected void AddItem(ListEntry item)
        {
            item.Destroy += RemoveItem;
            this.entries.Items.Add(item);
        }

        protected void AddItem(int index)
        {
            var item = new ListEntry(getValue, setValue, index);
            AddItem(item);
        }

        protected override void CreateItem()
        {
            var list = getValue();
            list.Add(null);
            setValue(list);

            AddItem(list.Count - 1);
        }

        protected override void RemoveItem(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void Refresh()
        {
            entries.Items.Clear();
            for (int i = 0; i < this.getValue().Count; i++)
                AddItem(i);
        }
    }
}