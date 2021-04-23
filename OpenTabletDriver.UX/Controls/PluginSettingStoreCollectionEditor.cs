using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreCollectionEditor<TSource> : Panel
    {
        public PluginSettingStoreCollectionEditor(
            PluginSettingStoreCollection storeCollection,
            string friendlyName = null
        )
        {
            this.baseControl.Panel1 = new Scrollable { Content = sourceSelector };
            this.baseControl.Panel2 = new Scrollable { Content = settingStoreEditor };

            sourceSelector.SelectedValueChanged += (sender, e) => SelectStore(sourceSelector.SelectedItem);
            this.FriendlyTypeName = friendlyName;

            StoreCollection = storeCollection;
        }

        private PluginSourceSelector sourceSelector = new PluginSourceSelector();
        private ToggleablePluginSettingStoreEditor settingStoreEditor = new ToggleablePluginSettingStoreEditor();

        private Splitter baseControl = new Splitter
        {
            Panel1MinimumSize = 200,
            Orientation = Orientation.Horizontal,
            BackgroundColor = SystemColors.WindowBackground
        };

        public string FriendlyTypeName { protected set; get; }

        private PluginSettingStoreCollection storeCollection;
        public PluginSettingStoreCollection StoreCollection
        {
            set
            {
                this.storeCollection = value;
                this.sourceSelector.Refresh();
                this.Content = this.sourceSelector.DataStore.Any() ? baseControl : new PluginSettingStoreEmptyPlaceholder(FriendlyTypeName);
            }
            get => this.storeCollection;
        }

        private void SelectStore(PluginReference reference)
        {
            if (StoreCollection != null && reference != null)
            {
                if (StoreCollection.FirstOrDefault(store => store.Path == reference.Path) is PluginSettingStore store)
                {
                    settingStoreEditor.Refresh(store);
                }
                else
                {
                    var newStore = new PluginSettingStore(reference.GetTypeReference<TSource>(), false);
                    StoreCollection.Add(newStore);
                    settingStoreEditor.Refresh(newStore);
                }
            }
        }

        private class PluginSettingStoreEmptyPlaceholder : Panel
        {
            public PluginSettingStoreEmptyPlaceholder(string friendlyName)
            {
                string pluginTypeName = string.IsNullOrWhiteSpace(friendlyName) ? typeof(TSource).Name : $"{friendlyName.ToLower()}s";
                this.Content = new StackView
                {
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new StackLayoutItem
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Control = new Bitmap(App.Logo.WithSize(256, 256))
                        },
                        new StackLayoutItem(null, true),
                        new StackLayoutItem
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Control = $"No plugins containing {pluginTypeName} are installed."
                        },
                        new StackLayoutItem
                        {
                            Control = new Button
                            {
                                Text = "Open Plugin Manager",
                                Command = new Command(
                                    (s, e) => (Application.Instance.MainForm as MainForm).ShowPluginManager()
                                )
                            },
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new StackLayoutItem(null, true)
                    }
                };
            }
        }

        private class ToggleablePluginSettingStoreEditor : PluginSettingStoreEditor<TSource>
        {
            protected override IEnumerable<Control> GetHeaderControlsForStore(PluginSettingStore store)
            {
                var enableButton = new CheckBox
                {
                    Text = $"Enable {store.GetPluginReference().Name ?? store.Path}",
                    Checked = store.Enable
                };
                enableButton.CheckedChanged += (sender, e) => store.Enable = enableButton.Checked ?? false;
                yield return enableButton;
            }
        }

        private class PluginSourceSelector : ListBox<PluginReference>
        {
            public PluginSourceSelector()
            {
                this.ItemTextBinding = Binding.Property<PluginReference, string>(p => p.Name ?? p.Path);
                Refresh();
            }

            public void Refresh()
            {
                var items = from type in AppInfo.PluginManager.GetChildTypes<TSource>()
                    select new PluginReference(AppInfo.PluginManager, type);

                this.Source = items.ToList();

                var lastIndex = this.SelectedIndex;
                this.SelectedIndex = lastIndex;
            }
        }
    }
}
