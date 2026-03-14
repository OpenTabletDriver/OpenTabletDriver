using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic.Reflection;

namespace OpenTabletDriver.UX.Controls
{
    public class PluginSettingStoreCollectionEditor<TSource> : Panel where TSource : class
    {
        public PluginSettingStoreCollectionEditor()
        {
            this.Content = placeholder = new Placeholder
            {
                Text = "No plugins containing this type are installed.",
                ExtraContent = new Button
                {
                    Text = "Open Plugin Manager",
                    Command = new Command((s, e) => App.Current.PluginManagerWindow.Show())
                }
            };

            mainContent = new Splitter
            {
                Panel1MinimumSize = 150,
                Panel1 = new Scrollable
                {
                    Border = BorderType.None,
                    Content = sourceSelector = new TypeListBox<TSource>()
                },
                Panel2 = new Scrollable
                {
                    Content = settingStoreEditor = new ToggleablePluginSettingStoreEditor()
                    {
                        Padding = 5
                    }
                }
            };

            settingStoreEditor.StoreBinding.Bind(
                sourceSelector.SelectedItemBinding.Convert(t => StoreCollection?.FromType(t))
            );

            if (!Platform.IsMac) // Don't do this on macOS, causes poor UI performance.
                settingStoreEditor.BackgroundColor = SystemColors.WindowBackground;

            AppInfo.PluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        private Placeholder placeholder;
        private Splitter mainContent;
        private TypeListBox<TSource> sourceSelector;
        private ToggleablePluginSettingStoreEditor settingStoreEditor;

        private PluginSettingStoreCollection storeCollection;
        public PluginSettingStoreCollection StoreCollection
        {
            set
            {
                this.storeCollection = value;
                this.OnStoreCollectionChanged();
            }
            get => this.storeCollection;
        }

        public event EventHandler<EventArgs> StoreCollectionChanged;

        protected virtual void OnStoreCollectionChanged()
        {
            StoreCollectionChanged?.Invoke(this, new EventArgs());
            RefreshContent();
        }

        private void HandleAssembliesChanged(object sender, EventArgs e) => Application.Instance.AsyncInvoke(RefreshContent);

        private void RefreshContent()
        {
            var types = AppInfo.PluginManager.GetChildTypes<TSource>();
            var sortedTypes = types.OrderBy(t => t.GetFriendlyName());

            // On Unix based platforms, the last plugin in the list may be hidden behind an horizontal scrollbar
            // We add another elements as padding to avoid this.
            var finalTypes = OperatingSystem.IsWindows() ? new ReadOnlyCollection<TypeInfo>([.. sortedTypes]) :
                                                           new ReadOnlyCollection<TypeInfo>([.. sortedTypes, null]);

            // Select the last selected plugin, or none if last selected was removed or none were selected.
            var oldSelected = sourceSelector.SelectedItem;
            var newSelected = finalTypes.FirstOrDefault(t => t?.FullName == oldSelected?.FullName) ?? finalTypes.FirstOrDefault();

            // Update DataStore to new types, this refreshes the editor.
            sourceSelector.SelectedItem = null;
            sourceSelector.DataStore = finalTypes;
            sourceSelector.SelectedItem = newSelected;

            this.Content = types.Any() ? mainContent : placeholder;
        }

        public BindableBinding<PluginSettingStoreCollectionEditor<TSource>, PluginSettingStoreCollection> StoreCollectionBinding
        {
            get
            {
                return new BindableBinding<PluginSettingStoreCollectionEditor<TSource>, PluginSettingStoreCollection>(
                    this,
                    c => c.StoreCollection,
                    (c, v) => c.StoreCollection = v,
                    (c, h) => c.StoreCollectionChanged += h,
                    (c, h) => c.StoreCollectionChanged -= h
                );
            }
        }

        private class ToggleablePluginSettingStoreEditor : PluginSettingStoreEditor<TSource>
        {
            protected override IEnumerable<Control> GetHeaderControlsForStore(PluginSettingStore store)
            {
                var enableButton = new CheckBox
                {
                    Text = $"Enable {store.Name ?? store.Path}",
                    Checked = store.Enable
                };
                enableButton.CheckedChanged += (sender, e) => store.Enable = enableButton.Checked ?? false;
                yield return enableButton;
            }
        }
    }
}
