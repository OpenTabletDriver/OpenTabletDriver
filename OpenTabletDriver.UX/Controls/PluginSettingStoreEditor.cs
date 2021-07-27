using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreEditor<TSource> : Panel
    {
        public PluginSettingStoreEditor()
        {
            this.Content = layout = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };
        }

        private StackLayout layout;

        private PluginSettingStore store;
        public PluginSettingStore Store
        {
            set
            {
                this.store = value;
                this.OnStoreChanged();
            }
            get => this.store;
        }

        public event EventHandler<EventArgs> StoreChanged;

        protected virtual void OnStoreChanged()
        {
            StoreChanged?.Invoke(this, new EventArgs());

            layout.Items.Clear();
            if (Store != null)
            {
                foreach (var control in GetHeaderControlsForStore(Store).Concat(GetControlsForStore(Store)))
                {
                    layout.Items.Add(control);
                }
            }
        }

        public BindableBinding<PluginSettingStoreEditor<TSource>, PluginSettingStore> StoreBinding
        {
            get
            {
                return new BindableBinding<PluginSettingStoreEditor<TSource>, PluginSettingStore>(
                    this,
                    c => c.Store,
                    (c, v) => c.Store = v,
                    (c, h) => c.StoreChanged += h,
                    (c, h) => c.StoreChanged -= h
                );
            }
        }

        protected virtual IEnumerable<Control> GetHeaderControlsForStore(PluginSettingStore store)
        {
            return Array.Empty<Control>();
        }

        private IEnumerable<Control> GetControlsForStore(PluginSettingStore store)
        {
            if (store != null)
            {
                var type = store.GetPluginReference().GetTypeReference<TSource>();
                return GetControlsForType(store, type);
            }
            else
            {
                return Array.Empty<Control>();
            }
        }

        private IEnumerable<Control> GetControlsForType(PluginSettingStore store, Type type)
        {
            var properties = from property in type.GetProperties()
                let attrs = property.GetCustomAttributes(true)
                where attrs.Any(a => a is PropertyAttribute)
                select property;

            foreach (var property in properties)
                yield return GeneratedControls.GetControlForProperty(store, property);
        }
    }
}
