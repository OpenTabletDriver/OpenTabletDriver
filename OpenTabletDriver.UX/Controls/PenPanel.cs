using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class PenPanel : BindingPanel
    {
        public PenPanel(IControlBuilder controlBuilder, App app) : base(controlBuilder)
        {
            var tipSettings = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    ButtonFor(p => p.BindingSettings.TipButton),
                    SliderFor(p => p.BindingSettings.TipActivationThreshold)
                }
            };

            var eraserSettings = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    ButtonFor(p => p.BindingSettings.EraserButton),
                    SliderFor(p => p.BindingSettings.EraserActivationThreshold)
                }
            };

            var tips = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(tipSettings, true),
                    new StackLayoutItem(eraserSettings, true)
                }
            };

            var buttons = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            DataContextChanged += delegate
            {
                buttons.Items.Clear();

                if (DataContext is not Profile profile)
                    return;

                var tablet = app.GetTablet(profile);
                var penButtonCount = tablet.Specifications.Pen?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.BindingSettings.PenButtons, penButtonCount))
                    buttons.Items.Add(button);
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    tips,
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Scrollable
                        {
                            Content = buttons
                        }
                    }
                }
            };
        }
    }
}
