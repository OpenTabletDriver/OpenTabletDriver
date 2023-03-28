using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class AuxPanel : BindingPanel
    {
        public AuxPanel(IControlBuilder controlBuilder, App app) : base(controlBuilder)
        {
            var outerContainer = new Scrollable();

            var buttons = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            var wheelButtons = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5,
                Items =
                {
                    ButtonFor(p => p.BindingSettings.WheelClockwise),
                    ButtonFor(p => p.BindingSettings.WheelCounterClockwise),
                }
            };

            var combinedButtons = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new GroupBox
                    {
                        Text = "Auxiliary buttons",
                        Content = buttons
                    },
                    new GroupBox
                    {
                        Text = "Wheel pseudobuttons",
                        Content = wheelButtons
                    }
                }
            };

            DataContextChanged += delegate
            {
                buttons.Items.Clear();

                if (DataContext is not Profile profile)
                    return;

                var tablet = app.Tablets.First(t => t.Name == profile.Tablet);
                var buttonCount = tablet.Specifications.AuxiliaryButtons?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.BindingSettings.AuxButtons, buttonCount))
                    buttons.Items.Add(button);

                //Only show the full panel if a Wheel is detected. Otherwise, just show the buttons
                outerContainer.Content = tablet.Specifications.Wheel != null
                    ? combinedButtons
                    : buttons;

            };

            Content = outerContainer;
        }
    }
}
