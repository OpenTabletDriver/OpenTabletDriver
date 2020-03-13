using System.Collections.ObjectModel;
using System.Linq;
using OpenTabletDriver.Plugins;
using TabletDriverLib;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Resident;

namespace OpenTabletDriver.Controls
{
    public class ResidentPluginEditorViewModel : PluginSettingsEditorViewModel<IResident>
    {
        public override void Refresh()
        {
            var plugins = from plugin in PluginManager.GetChildTypes<IResident>()
                where !plugin.IsInterface
                where !plugin.GetCustomAttributes(false).Any(a => a is PluginIgnoreAttribute)
                select plugin;
            
            var pluginList = new Collection<SelectablePluginReference>();
            foreach (var plugin in plugins)
            {
                bool isEnabled = Settings?.ResidentPlugins?.Any(r => r == plugin.FullName) ?? false;
                pluginList.Add(new SelectablePluginReference(plugin.FullName, isEnabled));
            }

            Plugins = new ObservableCollection<SelectablePluginReference>(pluginList);
        }
    }
}