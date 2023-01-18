using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class SettingsPanel : DesktopPanel
    {
        public SettingsPanel(App app, IControlBuilder controlBuilder)
        {
            var tabletPicker = controlBuilder.Build<TabletPicker>();

            var toolbar = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new Panel
                    {
                        MinimumSize = new Size(250, 0),
                        Content = tabletPicker
                    },
                    new StackLayoutItem(null, true),
                    new Button((_, _) => app.DiscardSettings().Run())
                    {
                        Text = "Discard"
                    },
                    new Button((_, _) => app.SaveSettings().Run())
                    {
                        Text = "Save"
                    },
                    new Button((_, _) => app.ApplySettings().Run())
                    {
                        Text = "Apply"
                    }
                }
            };

            var tabletPanel = controlBuilder.Build<TabletPanel>();
            tabletPanel.DataContextBinding.Bind(tabletPicker.SelectedValueBinding);

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = tabletPanel
                    },
                    new StackLayoutItem
                    {
                        Control = toolbar
                    }
                }
            };
        }
    }
}
