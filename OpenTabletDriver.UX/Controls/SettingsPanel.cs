using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class SettingsPanel : DesktopPanel
    {
        public SettingsPanel(App app, IControlBuilder controlBuilder)
        {
            var profilePicker = controlBuilder.Build<ProfilePicker>();

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
                        Content = profilePicker
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

            // TODO: Fix profile switching retaining old settings while updating the profile (such as lock aspect ratio)
            var profilePanel = controlBuilder.Build<ProfilePanel>();
            profilePanel.DataContextBinding.Bind(profilePicker.SelectedValueBinding);

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = profilePanel
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
