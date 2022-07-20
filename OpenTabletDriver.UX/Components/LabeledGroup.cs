using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Components
{
    public class LabeledGroup : DesktopPanel
    {
        public LabeledGroup(string text, Control? primaryControl, params StackLayoutItem[] additionalControls)
        {
            var layout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Padding(5, 0),
                Spacing = 10,
                Items =
                {
                    text,
                    new StackLayoutItem(primaryControl, true)
                }
            };

            foreach (var suffix in additionalControls)
                layout.Items.Add(suffix);

            Content = new GroupBox
            {
                Padding = 5,
                BackgroundColor = Platform.IsMac ? default : SystemColors.WindowBackground,
                Content = layout
            };
        }
    }
}
