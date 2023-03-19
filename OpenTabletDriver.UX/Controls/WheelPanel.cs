using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class WheelPanel : BindingPanel
    {
        private const int DIRECTION_PSEUDOBUTTONS = 2;

        public WheelPanel(IControlBuilder controlBuilder, App app) : base(controlBuilder)
        {
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

                var tablet = app.Tablets.First(t => t.Name == profile.Tablet);
                var buttonCount = DIRECTION_PSEUDOBUTTONS + tablet.Specifications.Wheel?.StepCount ?? 0;

                foreach (var button in ButtonsFor(c => c.BindingSettings.Wheel, buttonCount))
                    buttons.Items.Add(button);
            };

            Content = new Scrollable
            {
                Content = buttons
            };
        }
    }
}
