using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class GeneratedItemList<T> : Panel
    {
        public GeneratedItemList()
        {
            this.Content = layout;
        }

        protected StackLayout layout = new StackLayout
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 5
        };

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

        public BindableBinding<GeneratedItemList<T>, IList<T>> ItemSourceBinding
        {
            get
            {
                return new BindableBinding<GeneratedItemList<T>, IList<T>>(
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
        /// <param name="index">The index in which the item was sourced from.</param>
        /// <param name="itemBinding">A binding to the item sourced from <see cref="ItemSource"/>.</param>
        /// <returns>A control that handles the object</returns>
        protected abstract Control CreateControl(int index, DirectBinding<T> itemBinding);

        /// <summary>
        /// Internally used to create the base control item for the control created at <see cref="CreateControl"/>.
        /// </summary>
        /// <param name="index">The index in which the item was sourced from.</param>
        /// <param name="itemBinding">A binding to the item sourced from <see cref="ItemSource"/>.</param>
        /// <returns>A base control that handles the object</returns>
        protected virtual Control CreateControlBase(int index, DirectBinding<T> itemBinding) => CreateControl(index, itemBinding);

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
                    layout.Items.Clear();

                    int index = 0;
                    foreach (T item in ItemSource ?? Array.Empty<T>())
                    {
                        Insert(index);
                        index++;
                    }
                    break;
                }
            }
        }

        protected virtual void Insert(int index)
        {
            var itemBinding = new DelegateBinding<T>(
                () => ItemSource[index],
                (v) => ItemSource[index] = v
            );

            var control = CreateControlBase(index, itemBinding);
            layout.Items.Insert(index, control);
        }
    }
}
