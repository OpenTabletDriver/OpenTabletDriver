using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public sealed class Placeholder : DesktopPanel
    {
        public Placeholder(string text, Control? extraContent = null)
        {
            Content = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Bitmap(Metadata.Logo, 256, 256),
                    text,
                    extraContent,
                    new StackLayoutItem(null, true)
                }
            };
        }
    }
}
