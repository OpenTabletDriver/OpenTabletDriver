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
            PluginSettingStoreCollection store,
            string friendlyName = null
        )
        {
            this.baseControl.Panel1 = new Scrollable { Content = sourceSelector };
            this.baseControl.Panel2 = new Scrollable { Content = settingStoreEditor };

            sourceSelector.SelectedSourceChanged += (sender, reference) => UpdateSelectedStore(reference);
            this.FriendlyTypeName = friendlyName;

            UpdateStore(store);
        }

        private readonly string FriendlyTypeName;

        public WeakReference<PluginSettingStoreCollection> CollectionReference { protected set; get; }

        public void UpdateStore(PluginSettingStoreCollection storeCollection)
        {
            if (CollectionReference == null)
                CollectionReference = new WeakReference<PluginSettingStoreCollection>(storeCollection);
            else
                CollectionReference.SetTarget(storeCollection);
            sourceSelector.Refresh();
            
            if (sourceSelector.Plugins.Count == 0)
            {
                this.Content = new PluginSettingStoreEmptyPlaceholder(FriendlyTypeName);
            }
            else
            {
                this.Content = baseControl;
            }
        }

        private Splitter baseControl = new Splitter
        {
            Panel1MinimumSize = 200,
            Orientation = Orientation.Horizontal,
            BackgroundColor = SystemColors.WindowBackground
        };

        public PluginReference SelectedPlugin => sourceSelector.SelectedSource;

        private PluginSourceSelector sourceSelector = new PluginSourceSelector();
        private ToggleablePluginSettingStoreEditor settingStoreEditor = new ToggleablePluginSettingStoreEditor();

        private void UpdateSelectedStore(PluginReference reference)
        {
            if (CollectionReference.TryGetTarget(out PluginSettingStoreCollection storeCollection))
            {
                if (storeCollection.FirstOrDefault(store => store.Path == reference.Path) is PluginSettingStore store)
                {
                    settingStoreEditor.Refresh(store);
                }
                else
                {
                    var newStore = new PluginSettingStore(reference.GetTypeReference<TSource>(), false);
                    storeCollection.Add(newStore);
                    settingStoreEditor.Refresh(newStore);
                }
            }
        }

        private class PluginSettingStoreEmptyPlaceholder : StackView
        {
            public PluginSettingStoreEmptyPlaceholder(string friendlyName)
            {
                string pluginTypeName = string.IsNullOrWhiteSpace(friendlyName) ? typeof(TSource).Name : $"{friendlyName.ToLower()}s";
                base.Items.Add(new StackLayoutItem(null, true));
                base.Items.Add(
                    new StackLayoutItem
                    {
                        Control = new Bitmap(App.Logo.WithSize(256, 256)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                );
                base.Items.Add(
                    new StackLayoutItem
                    {
                        Control = $"No plugins containing {pluginTypeName} are installed.",
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                );
                base.Items.Add(
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
                    }
                );
                base.Items.Add(new StackLayoutItem(null, true));
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

        private class PluginSourceSelector : ListBox
        {
            public PluginSourceSelector()
            {
                Refresh();
            }

            public IList<PluginReference> Plugins { protected set; get; }

            public PluginReference SelectedSource { protected set; get; }

            public event EventHandler<PluginReference> SelectedSourceChanged;

            public void Refresh()
            {
                var items = from type in AppInfo.PluginManager.GetChildTypes<TSource>()
                    select new PluginReference(AppInfo.PluginManager, type);

                Plugins = items.ToList();

                this.DataStore = Plugins;
                this.ItemTextBinding = Binding.Property<PluginReference, string>(p => p.Name ?? p.Path);

                var lastIndex = this.SelectedIndex;
                this.SelectedIndex = -1;
                this.SelectedIndex = lastIndex;
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
                this.OnSelectedSourceChanged(e);
            }

            protected virtual void OnSelectedSourceChanged(EventArgs e)
            {
                if (this.SelectedIndex < 0 || this.SelectedIndex > Plugins.Count - 1)
                    return;

                SelectedSource = Plugins[this.SelectedIndex];
                SelectedSourceChanged?.Invoke(this, SelectedSource);
            }
        }
    }
}
