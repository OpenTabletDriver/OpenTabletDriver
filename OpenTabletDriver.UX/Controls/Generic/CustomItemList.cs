using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class CustomItemList<T> : StackLayout
    {
        private IList<T> itemSource;
        public IList<T> ItemSource
        {
            set
            {
                // Unhook the old collection change event if applicable
                if (ItemSource is INotifyCollectionChanged oldNotify)
                    oldNotify.CollectionChanged -= HandleCollectionChanged;

                this.itemSource = value;
                this.OnItemSourceChanged();

                // Hook the collection change event if applicable
                if (ItemSource is INotifyCollectionChanged newNotify)
                    newNotify.CollectionChanged += HandleCollectionChanged;
            }
            get => this.itemSource;
        }

        public event EventHandler<EventArgs> ItemSourceChanged;

        public BindableBinding<CustomItemList<T>, IList<T>> ItemSourceBinding
        {
            get
            {
                return new BindableBinding<CustomItemList<T>, IList<T>>(
                    this,
                    c => c.ItemSource,
                    (c, v) => c.ItemSource = v,
                    (c, h) => c.ItemSourceChanged += h,
                    (c, h) => c.ItemSourceChanged -= h
                );
            }
        }

        /// <summary>
        /// Creates a control for an instance of a T in the enumerable.
        /// </summary>
        /// <param name="sourceObj">The object in which the control is to be created for.</param>
        /// <returns>A control that handles the object</returns>
        protected abstract Control CreateControl(int index, T sourceObj);

        protected virtual void OnItemSourceChanged()
        {
            ItemSourceChanged?.Invoke(this, new EventArgs());
            HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                // Others are not implemented right now, no use case we currently have requires their implementation.
                case NotifyCollectionChangedAction.Reset:
                default:
                {
                    this.Items.Clear();

                    int index = 0;
                    foreach (T item in ItemSource ?? Array.Empty<T>())
                    {
                        Insert(index, item);
                        index++;
                    }
                    break;
                }
            }
        }

        protected virtual void Insert(int index, T item)
        {
            var control = CreateControl(index, (T)item);
            control.DataContext = item;

            this.Items.Insert(index, control);
        }
    }
}