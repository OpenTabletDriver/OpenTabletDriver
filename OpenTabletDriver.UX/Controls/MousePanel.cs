using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class MousePanel : BindingPanel
    {
        public MousePanel(IControlBuilder controlBuilder, App app) : base(controlBuilder)
        {
            var scrollButtons = new GroupBox
            {
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Padding = new Padding(5, 5, 5, 0),
                    Spacing = 5,
                    Items =
                    {
                        ButtonFor(p => p.BindingSettings.MouseScrollUp),
                        ButtonFor(p => p.BindingSettings.MouseScrollDown)
                    }
                }
            };

            var buttons = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5
            };

            DataContextChanged += delegate
            {
                buttons.Items.Clear();

                if (DataContext is not Profile profile)
                    return;

                var tablet = app.GetTablet(profile);
                var buttonCount = tablet.Specifications.MouseButtons?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.BindingSettings.MouseButtons, buttonCount))
                    buttons.Items.Add(button);
            };

            Content = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    scrollButtons,
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new GroupBox
                        {
                            Content = new Scrollable
                            {
                                Content = buttons
                            }
                        }
                    }
                }
            };
        }
    }
}
