using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DictionaryEditor : CollectionEditor<Dictionary<string, string>>
    {
        public DictionaryEditor(
            string name,
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue
        ) : base(name, getValue, setValue)
        {
        }

        protected void AddItem(KeyValuePair<string, string> pair)
        {
            var item = new DictionaryEntry(getValue, setValue, pair.Key, pair.Value);
            base.AddItem(item);
        }

        protected override void CreateItem()
        {
            var item = new DictionaryEntry(getValue, setValue);
            base.AddItem(item);
        }

        protected override void RemoveItem(object sender, EventArgs e)
        {
            if (sender is StackLayoutItem stackLayoutItem)
                this.entries.Items.Remove(stackLayoutItem);
        }

        protected override void Refresh()
        {
            this.entries.Items.Clear();
            foreach (var pair in getValue())
                AddItem(pair);
        }
    }
}