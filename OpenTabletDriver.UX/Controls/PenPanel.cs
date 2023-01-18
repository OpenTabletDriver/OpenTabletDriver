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
                    ButtonFor(p => p.Profile.Bindings.TipButton),
                    SliderFor(p => p.Profile.Bindings.TipActivationThreshold)
                }
            };

            var eraserSettings = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    ButtonFor(p => p.Profile.Bindings.EraserButton),
                    SliderFor(p => p.Profile.Bindings.EraserActivationThreshold)
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

                if (DataContext is not TabletHandler tabletHandler)
                    return;

                var penButtonCount = tabletHandler.Configuration.Specifications.Pen?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.Profile.Bindings.PenButtons, penButtonCount))
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
