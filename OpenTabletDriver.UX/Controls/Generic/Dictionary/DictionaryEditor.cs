using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic.Dictionary
{
    public abstract class DictionaryEditor<TKey, TValue> : Panel
    {
        public DictionaryEditor()
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
                    ItemSource = new Dictionary<TKey, TValue>();
                AddNew();
            };
        }

        private Button add;

        protected StackLayout layout = new StackLayout
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 5
        };

        private IDictionary<TKey, TValue> itemSource;
        public IDictionary<TKey, TValue> ItemSource
        {
            set
            {
                this.itemSource = value;
                this.OnItemSourceChanged();
            }
            get => this.itemSource;
        }

        public event EventHandler<EventArgs> ItemSourceChanged;

        protected virtual void OnItemSourceChanged()
        {
            ItemSourceChanged?.Invoke(this, new EventArgs());
            HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public BindableBinding<DictionaryEditor<TKey, TValue>, IDictionary<TKey, TValue>> ItemSourceBinding
        {
            get
            {
                return new BindableBinding<DictionaryEditor<TKey, TValue>, IDictionary<TKey, TValue>>(
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
        protected abstract Control CreateControl(DirectBinding<TKey> keyBinding, DirectBinding<TValue> valueBinding);

        /// <summary>
        /// Internally used to create the base control item for the control created at <see cref="CreateControl"/>.
        /// </summary>
        /// <param name="keyBinding">A binding that specifies the key in the dictionary</param>
        /// <param name="valueBinding">A binding that modifies the <see cref="keyBinding"/>'s value in the dictionary.</param>
        /// <returns>A base control that handles the object</returns>
        protected virtual Control CreateControlBase(DirectBinding<TKey> keyBinding, DirectBinding<TValue> valueBinding)
        {
            var remove = new Button
            {
                Text = "-"
            };
            remove.Click += (sender, e) => Remove(keyBinding.DataValue);

            return new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Top,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = CreateControl(keyBinding, valueBinding)
                    },
                    new StackLayoutItem
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Control = remove
                    }
                }
            };
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
                    foreach (KeyValuePair<TKey, TValue> item in ItemSource ?? new Dictionary<TKey, TValue>())
                    {
                        Insert(item.Key);
                        index++;
                    }
                    break;
                }
            }
        }

        protected virtual void Insert(TKey key)
        {
            var keyBinding = new DelegateBinding<TKey>(
                () => key,
                (newKey) =>
                {
                    var value = ItemSource[key];
                    ItemSource.Remove(key);
                    ItemSource.Add(newKey, value);
                    key = newKey;
                }
            );

            var valueBinding = new DelegateBinding<TValue>(
                () => ItemSource[key],
                (newValue) => ItemSource[key] = newValue
            );

            var control = CreateControlBase(keyBinding, valueBinding);
            var index = layout.Items.Count;
            layout.Items.Insert(index, control);
        }

        protected abstract void AddNew();

        protected virtual void Add(TKey key, TValue value)
        {
            if (ItemSource.TryAdd(key, value))
            {
                var pair = ItemSource.First(t => t.Key.Equals(key));
                if (!(ItemSource is INotifyCollectionChanged))
                    HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, pair));
            }
        }

        protected virtual void Remove(TKey key)
        {
            var oldObj = ItemSource.First(k => k.Key.Equals(key));
            ItemSource.Remove(key);

            if (ItemSource.Count == 0)
                ItemSource = null;

            if (!(ItemSource is INotifyCollectionChanged))
                HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldObj));
        }
    }
}
