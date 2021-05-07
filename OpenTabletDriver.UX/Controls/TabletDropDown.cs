using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Controls
{
    public class TabletDropDown : DropDown
    {
        public TabletDropDown()
        {
            this.ItemTextBinding = Binding.Delegate((TabletHandlerID id) => tablets.GetName(id));

            App.Current.ProfileCache.HandlerInFocusChanged += (_, _) =>
            {
                Application.Instance.AsyncInvoke(() => SelectedID = App.Current.ProfileCache.HandlerInFocus);
            };

            App.Driver.Instance.TabletHandlerCreated += async (_, id) => await tablets.Add(id);
            App.Driver.Instance.TabletHandlerDestroyed += (_, id) => tablets.Remove(id);

            InitializeAsync();
        }

        public event EventHandler<EventArgs> SelectedIDChanged;
        public event EventHandler<EventArgs> Initialized;

        public TabletHandlerID SelectedID
        {
            get => SelectedValue != null ? (TabletHandlerID)SelectedValue : TabletHandlerID.Invalid;
            set
            {
                SelectedValue = (object)value;
                SelectedIDChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BindableBinding<TabletDropDown, TabletHandlerID> SelectedIDBinding
        {
            get
            {
                return new BindableBinding<TabletDropDown, TabletHandlerID>(
                    this,
                    t => t.SelectedID,
                    (t, i) => t.SelectedID = i,
                    (t, h) => t.SelectedIDChanged += h,
                    (t, h) => t.SelectedIDChanged -= h
                );
            }
        }

        private TabletList tablets = new TabletList();

        private async void InitializeAsync()
        {
            foreach (var tablet in await App.Driver.Instance.GetActiveTabletHandlerIDs())
            {
                await tablets.Add(tablet);
            }

            this.DataStore = tablets;
            this.SelectedValue = App.Current.ProfileCache.HandlerInFocus;

            Initialized?.Invoke(this, EventArgs.Empty);
        }

        private class TabletList : INotifyCollectionChanged, IEnumerable<object>
        {
            public TabletList()
            {
                observer.CollectionChanged += (_, args) => Application.Instance.AsyncInvoke(() => CollectionChanged?.Invoke(this, args));
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            private Dictionary<TabletHandlerID, string> tablets = new Dictionary<TabletHandlerID, string>();

            private ObservableCollection<TabletHandlerID> observer = new ObservableCollection<TabletHandlerID>();

            public async Task Add(TabletHandlerID id)
            {
                var tabletState = await App.Driver.Instance.GetTablet(id);
                if (tablets.TryAdd(id, tabletState.Properties.Name))
                {
                    observer.Add(id);
                }
            }

            public string GetName(TabletHandlerID id)
            {
                return tablets[id];
            }

            public void Remove(TabletHandlerID id)
            {
                if (tablets.Remove(id))
                {
                    observer.Remove(id);
                }
            }

            public IEnumerator<object> GetEnumerator()
            {
                return observer.Select(k => k as object).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return observer.GetEnumerator();
            }
        }
    }
}