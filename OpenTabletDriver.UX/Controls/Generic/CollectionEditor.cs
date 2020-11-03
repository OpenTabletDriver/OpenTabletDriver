using System;
using System.Collections;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class CollectionEditor<T> : GroupBoxBase where T : IEnumerable
    {
        protected CollectionEditor(string name, Func<T> getValue, Action<T> setValue)
        {
            this.getValue = getValue;
            this.setValue = setValue;

            this.entries = new StackView();
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
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(entries, HorizontalAlignment.Stretch),
                    new StackLayoutItem(actions, HorizontalAlignment.Right)
                }
            };

            Refresh();

            base.Text = name;
        }

        protected abstract void CreateItem();
        protected abstract void RemoveItem(object sender, EventArgs e);
        protected abstract void Refresh();

        protected Func<T> getValue;
        protected Action<T> setValue;

        protected StackLayout entries, actions;
    }
}