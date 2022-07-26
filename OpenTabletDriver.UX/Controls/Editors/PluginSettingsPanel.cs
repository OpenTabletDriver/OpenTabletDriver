using System.Collections.Immutable;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls.Editors
{
    public abstract class PluginSettingsPanel<T> : DesktopPanel where T : class
    {
        private readonly IPluginFactory _pluginFactory;
        private readonly IControlBuilder _controlBuilder;
        private readonly App _app;
        private readonly ListBox<TypeInfo> _list;
        private readonly Splitter _splitter;
        private readonly Placeholder _placeholder;

        protected PluginSettingsPanel(
            IControlBuilder controlBuilder,
            IPluginFactory pluginFactory,
            App app,
            IPluginManager pluginManager
        )
        {
            _controlBuilder = controlBuilder;
            _pluginFactory = pluginFactory;
            _app = app;

            _splitter = new Splitter
            {
                Panel1 = new Panel
                {
                    MinimumSize = new Size(250, 0),
                    Content = _list = new ListBox<TypeInfo>
                    {
                        ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName!),
                        ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName() ?? t.FullName!)
                    }
                },
                Panel2 = _placeholder = new Placeholder("No plugin selected.")
            };

            pluginManager.AssembliesChanged += (_, _) => Refresh();
            DataContextChanged += (_, _) => UpdateContent();
            _list.SelectedIndexChanged += (_, _) => UpdateContent();

            Refresh();
        }

        protected abstract PluginSettingsCollection? Settings { get; }

        private void Refresh()
        {
            var items = _pluginFactory.GetMatchingTypes(typeof(T)).ToImmutableArray();
            _list.DataStore = items;

            if (items.Any())
            {
                Content = _splitter;
                UpdateContent();
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

        private void UpdateContent()
        {
            var type = _list.SelectedItem;

            if (Settings?.FromType(type) is PluginSettings settings)
                _splitter.Panel2 = _controlBuilder.Build<PluginSettingsEditor>(settings, type);
            else
                _splitter.Panel2 = _placeholder;
        }
    }
}
