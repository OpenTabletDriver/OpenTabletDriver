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
        }

        protected void AddItem(int index)
        {
            var item = new ListEntry(base.getValue, base.setValue, index);
            base.AddItem(item);
        }

        protected override void CreateItem()
        {
            var list = base.getValue();
            list.Add(null);
            base.setValue(list);

            AddItem(list.Count - 1);
        }

        protected override void RemoveItem(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void Refresh()
        {
            entries.Items.Clear();
            for (int i = 0; i < base.getValue().Count; i++)
                AddItem(i);
        }
    }
}