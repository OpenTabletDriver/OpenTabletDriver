using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DictionaryEditor : GroupBoxBase
    {
        public DictionaryEditor(
            string name,
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue
        ) : this()
        {
            this.getValue = getValue;
            this.setValue = setValue;

            this.actions.Items.Add(
                new StackLayoutItem
                {
                    Control = new Button((sender, e) => CreateItem())
                    {
                        Text = "+"
                    }
                }
            );

            foreach (var pair in getValue())
                AddItem(pair);

            base.Text = name;
        }

        protected DictionaryEditor()
        {
            this.entries = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            this.actions = new StackLayout
            {
                Orientation = Orientation.Horizontal
            };

            base.Content = new StackLayout
            {
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(entries, HorizontalAlignment.Stretch),
                    new StackLayoutItem(actions, HorizontalAlignment.Right)
                }
            };
        }

        protected void AddItem(DictionaryEntry item)
        {
            item.Destroy += RemoveItem;
            this.entries.Items.Add(item);
        }

        protected void AddItem(KeyValuePair<string, string> pair)
        {
            var item = new DictionaryEntry(getValue(), pair.Key, pair.Value);
            AddItem(item);
        }

        protected void CreateItem()
        {
            var item = new DictionaryEntry(getValue());
            AddItem(item);
        }

        protected void RemoveItem(object sender, EventArgs e)
        {
            if (sender is StackLayoutItem stackLayoutItem)
                this.entries.Items.Remove(stackLayoutItem);
        }

        private Func<Dictionary<string, string>> getValue;
        private Action<Dictionary<string, string>> setValue;
        private StackLayout entries, actions;
    }
}