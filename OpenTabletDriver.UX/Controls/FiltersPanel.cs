using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;

namespace OpenTabletDriver.UX.Controls
{
    public class FiltersPanel : PluginSettingsPanel<IDevicePipelineElement>
    {
        public FiltersPanel(
            IControlBuilder controlBuilder,
            IPluginFactory pluginFactory,
            App app,
            IPluginManager pluginManager
        ) : base(controlBuilder, pluginFactory, app, pluginManager)
        {
        }

        protected override PluginSettingsCollection? Settings => (DataContext as TabletHandler)?.Profile.Filters;
    }
}
