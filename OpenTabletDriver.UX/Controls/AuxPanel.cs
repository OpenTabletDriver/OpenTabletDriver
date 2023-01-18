using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class AuxPanel : BindingPanel
    {
        public AuxPanel(IControlBuilder controlBuilder, App app) : base(controlBuilder)
        {
            var buttons = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };

            DataContextChanged += delegate
            {
                buttons.Items.Clear();

                if (DataContext is not TabletHandler handler)
                    return;

                var profile = handler.Profile;
                var tablet = app.Tablets.First(t => t.Name == profile.Tablet);
                var buttonCount = tablet.Configuration.Specifications.AuxiliaryButtons?.ButtonCount ?? 0;

                foreach (var button in ButtonsFor(c => c.Profile.Bindings.AuxButtons, buttonCount))
                    buttons.Items.Add(button);
            };

            Content = new Scrollable
            {
                Content = buttons
            };
        }
    }
}
