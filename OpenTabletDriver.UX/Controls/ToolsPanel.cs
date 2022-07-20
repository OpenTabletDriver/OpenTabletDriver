using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;

namespace OpenTabletDriver.UX.Controls
{
    public class ToolsPanel : DesktopPanel
    {
        private readonly PluginSettingsEditorList<ITool> _editor;
        private readonly App _app;

        public ToolsPanel(IControlBuilder controlBuilder, App app)
        {
            _app = app;
            Content = _editor = controlBuilder.Build<PluginSettingsEditorList<ITool>>();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            _editor.DataContext = _app.Settings.Tools;
        }
    }
}
