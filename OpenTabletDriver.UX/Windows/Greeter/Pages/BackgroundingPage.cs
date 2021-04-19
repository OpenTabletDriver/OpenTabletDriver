using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Running in the background")]
    public class BackgroundingPage : StylizedPage
    {
        public BackgroundingPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(150, 150)),
                new StylizedText("Running in the background", SystemFonts.Bold(12), new Padding(0, 0, 0, 8)),
                "You can run OpenTabletDriver in the background.",
                "All you have to do is minimize it, and it will stay running in your system tray.",
                new PaddingSpacerItem(),
            };
        }
    }
}