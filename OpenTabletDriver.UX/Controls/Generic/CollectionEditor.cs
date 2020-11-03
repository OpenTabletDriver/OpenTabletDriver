using System;
using System.Collections;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class CollectionEditor<T> : GroupBoxBase where T : IEnumerable
    {
        protected CollectionEditor(string name, Func<T> getValue, Action<T> setValue) : base()
        {
            this.getValue = getValue;
            this.setValue = setValue;

            this.entries = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            this.actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items = 
                {   
                    new StackLayoutItem
                    {
                        Control = new Button((sender, e) => CreateItem())
                        {
                            Text = "+"
                        }
                    }
                }
            };

            base.Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(entries, true),
                    new StackLayoutItem(actions, VerticalAlignment.Bottom)
                }
            };

            Refresh();

            base.Text = name;
        }

        protected virtual void AddItem(CollectionEntry<T> entry)
        {
            entry.Destroy += RemoveItem;
            this.entries.Items.Add(entry);
        }

        protected abstract void CreateItem();
        protected abstract void RemoveItem(object sender, EventArgs e);
        protected abstract void Refresh();

        protected Func<T> getValue;
        protected Action<T> setValue;

        protected StackLayout entries, actions;
    }
}