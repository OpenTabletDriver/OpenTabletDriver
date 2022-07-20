using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;

namespace OpenTabletDriver.UX.Controls
{
    public class FiltersPanel : DesktopPanel
    {
        public FiltersPanel(IControlBuilder controlBuilder)
        {
            var editor = controlBuilder.Build<PluginSettingsEditorList<IDevicePipelineElement>>();
            editor.DataContextBinding.BindDataContext((Profile p) => p.Filters);
            Content = editor;
        }
    }
}
