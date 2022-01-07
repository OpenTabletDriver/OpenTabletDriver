using System.Collections.Generic;
using System.Collections.Specialized;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class ModifiableItemList<T> : GeneratedItemList<T>
    {
        public ModifiableItemList()
        {
            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = layout
                    },
                    new StackLayoutItem
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Control = add = new Button
                        {
                            Text = "+"
                        }
                    }
                }
            };

            add.Click += (sender, e) =>
            {
                if (ItemSource == null)
                    ItemSource = new List<T>();
                AddNew();
            };
        }

        private Button add;

        /// <summary>
        /// Invoked by the user to request addition of a new object at a specific index.
        /// </summary>
        protected abstract void AddNew();

        /// <summary>
        /// Invoked internally to add a new object at a specific
        /// </summary>
        /// <param name="index">The index in which the item will be added.</param>
        /// <param name="obj">The object to insert at the index.</param>
        protected virtual void Add(int index, T obj)
        {
            ItemSource.Insert(index, obj);
            if (!(ItemSource is INotifyCollectionChanged))
                HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj, index));
        }

        /// <summary>
        /// Invoked by the user to request deletion of an object at a specific index.
        /// </summary>
        /// <param name="index">The index in which the item will be removed.</param>
        protected virtual void Remove(int index)
        {
            var oldObj = ItemSource[index];
            ItemSource.RemoveAt(index);

            if (ItemSource.Count == 0)
                ItemSource = null;

            if (!(ItemSource is INotifyCollectionChanged))
                HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldObj, index));
        }

        protected override Control CreateControlBase(int index, DirectBinding<T> itemBinding)
        {
            var remove = new Button
            {
                Text = "-"
            };
            remove.Click += (sender, e) => Remove(index);

            return new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Top,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = base.CreateControlBase(index, itemBinding)
                    },
                    new StackLayoutItem
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Control = remove
                    }
                }
            };
        }
    }
}
