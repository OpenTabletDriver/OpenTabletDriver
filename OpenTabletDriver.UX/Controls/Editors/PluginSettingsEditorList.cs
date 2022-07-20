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
        // TODO: Fix discard/load not updating settings
        public PluginSettingsEditorList(IControlBuilder controlBuilder, IPluginFactory pluginFactory, App app)
        {
            var list = new ListBox<TypeInfo>
            {
                ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName!),
                ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName() ?? t.FullName!),
                DataStore = pluginFactory.GetMatchingTypes(typeof(T)).ToImmutableArray()
            };

            var settingsPanel = controlBuilder.Build<PluginSettingsEditor>();

            list.SelectedIndexChanged += (_, _) =>
            {
                if (DataContext is PluginSettingsCollection settingsCollection)
                {
                    var type = list.SelectedItem;
                    var settings = settingsCollection.FromType(type);
                    settingsPanel.DataContext = new SettingsViewModel(settings, type);
                }
            };

            if (list.DataStore.Any())
            {
                Content = new Splitter
                {
                    Panel1 = new Panel
                    {
                        MinimumSize = new Size(250, 0),
                        Content = list
                    },
                    Panel2 = settingsPanel
                };
            }
            else
            {
                Content = new Placeholder
                {
                    Text = "No plugins of this type are installed.",
                    ExtraContent = new Button((_, _) => app.ShowWindow<Windows.PluginManager>())
                    {
                        Text = "Show plugin manager..."
                    }
                };
            }
        }
    }
}
