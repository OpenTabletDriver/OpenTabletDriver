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
                        ButtonFor(p => p.Profile.Bindings.MouseScrollUp),
                        ButtonFor(p => p.Profile.Bindings.MouseScrollDown)
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

                if (DataContext is not TabletHandler tabletHandler)
                    return;

                var buttonCount = tabletHandler.Configuration.Specifications.MouseButtons?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.Profile.Bindings.MouseButtons, buttonCount))
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
