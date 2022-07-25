using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public class PluginSettingsEditorList<T> : DesktopPanel where T : class
    {
        private readonly ListBox<TypeInfo> _list;
        private readonly IPluginFactory _pluginFactory;
        private readonly PluginSettingsEditor _settingsPanel;
        private readonly App _app;

        // TODO: Fix discard/load not updating settings
        public PluginSettingsEditorList(IControlBuilder controlBuilder, IPluginFactory pluginFactory, IPluginManager pluginManager, App app)
        {
            _app = app;
            _pluginFactory = pluginFactory;

            _list = new ListBox<TypeInfo>
            {
                ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName!),
                ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName() ?? t.FullName!)
            };

            pluginManager.AssembliesChanged += (_, _) => Refresh();
            Refresh();

            _settingsPanel = controlBuilder.Build<PluginSettingsEditor>();

            _list.SelectedIndexChanged += (_, _) =>
            {
                var type = _list.SelectedItem;
                if (DataContext is PluginSettingsCollection settingsCollection && settingsCollection.FromType(type) is PluginSettings settings)
                    _settingsPanel.DataContext = new SettingsViewModel(settings, type);
                else
                    _settingsPanel.DataContext = null;
            };
        }

        private void Refresh()
        {
            var items = _pluginFactory.GetMatchingTypes(typeof(T)).ToImmutableArray();
            _list.DataStore = items;

            if (items.Any())
            {
                Content = new Splitter
                {
                    Panel1 = new Panel
                    {
                        MinimumSize = new Size(250, 0),
                        Content = _list
                    },
                    Panel2 = _settingsPanel
                };
            }
            else
            {
                var button = new Button((_, _) => _app.ShowWindow<Windows.PluginManager>())
                {
                    Text = "Show plugin manager..."
                };

                Content = new Placeholder("No plugins of this type are installed.", button);
            }
        }
    }
}
