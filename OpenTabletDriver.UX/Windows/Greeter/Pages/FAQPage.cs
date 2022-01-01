using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Wiki")]
    public class WikiPage : StylizedPage
    {
        public WikiPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(150, 150)),
                new StylizedText("Wiki", SystemFonts.Bold(12), new Padding(0, 0, 0, 8)),
                "If you have any issues, check out the Wiki.",
                "This can be found under the Help menu in the main window.",
                new PaddingSpacerItem(),
            };
        }
    }
}
