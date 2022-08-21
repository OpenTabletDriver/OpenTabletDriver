using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls.Editors;

namespace OpenTabletDriver.UX.Controls
{
    public class DigitizerPanel : DesktopPanel
    {
        public DigitizerPanel(IControlBuilder controlBuilder, IPluginFactory pluginFactory)
        {
            var editorPanel = new Panel();

            var outputModePicker = controlBuilder.Build<OutputModePicker>();
            outputModePicker.SelectedKeyBinding.BindDataContext((Profile p) => p.OutputMode.Path);
            outputModePicker.SelectedKeyChanged += delegate
            {
                if (DataContext is not Profile profile)
                    return;

                var type = pluginFactory.GetPluginType(profile.OutputMode.Path)!;
                if (type.IsAssignableTo(typeof(AbsoluteOutputMode)))
                {
                    if (editorPanel.Content is AbsoluteAreaEditor)
                        return;

                    editorPanel.Content = controlBuilder.Build<AbsoluteAreaEditor>();
                }
                else
                {
                    if (editorPanel.Content.ID == profile.OutputMode.Path)
                        return;

                    var layout = new StackLayout
                    {
                        ID = profile.OutputMode.Path,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Padding = 5,
                        Spacing = 5,
                    };

                    foreach (var control in controlBuilder.Generate(profile.OutputMode, type))
                        layout.Items.Add(control);

                    editorPanel.Content = layout;
                }
            };

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(editorPanel, true),
                    new StackLayoutItem(outputModePicker, HorizontalAlignment.Left)
                }
            };
        }
    }
}
