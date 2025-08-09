using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("System Tray")]
    public class SystemTrayPage : StylizedPage
    {
        public SystemTrayPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(150, 150)),
                new StylizedText("System Tray", SystemFonts.Bold(12), new Padding(0, 0, 0, 8)),
                "When minimized, OpenTabletDriver will run in the background.",
                "The window can be restored from the system tray.",
                new PaddingSpacerItem(),
            };
        }
    }
}
