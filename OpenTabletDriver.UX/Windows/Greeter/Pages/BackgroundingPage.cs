using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Running OpenTabletDriver in the background")]
    public class BackgroundingPage : StylizedPage
    {
        public BackgroundingPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(150, 150)),
                new StylizedText("Running OpenTabletDriver in the background", SystemFonts.Bold(12), new Padding(0, 0, 0, 8)),
                "Minimizing OpenTabletDriver will put it in your tray, allowing it to run in the background.",
                "",
                new PaddingSpacerItem(),
            };
        }
    }
}