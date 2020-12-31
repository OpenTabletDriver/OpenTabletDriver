using Eto.Drawing;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("FAQ")]
    public class FAQPage : StylizedPage
    {
        public FAQPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(256, 256)),
                "If you have any issues, check out the FAQ.",
                "This can be found under the Help menu in the main window.",
                new PaddingSpacerItem(),
            };
        }

        public const string FAQ_URL = "https://github.com/InfinityGhost/OpenTabletDriver/wiki#frequently-asked-questions";
    }
}