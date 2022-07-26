using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;

namespace OpenTabletDriver.UX.Controls
{
    public class ToolsPanel : PluginSettingsPanel<ITool>
    {
        private readonly App _app;

        public ToolsPanel(
            IControlBuilder controlBuilder,
            IPluginFactory pluginFactory,
            App app,
            IPluginManager pluginManager
        ) : base(controlBuilder, pluginFactory, app, pluginManager)
        {
            _app = app;
        }

        protected override PluginSettingsCollection? Settings => DataContext is null ? null : _app.Settings.Tools;
    }
}
