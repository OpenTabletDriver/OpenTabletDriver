using Eto.Drawing;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Plugins")]
    public class PluginPage : StylizedPage
    {
        public PluginPage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(150, 150)),
                new StylizedText("Plugins", SystemFonts.Bold(12), new Padding(0, 0, 0, 8)),
                "Plugins can be downloaded from the plugin manager at your own risk.",
                "The plugin manager can be found in the Plugins menu in the main window.",
                "Filters change the way output is handled on the tablet, an example of this is smoothing.",
                "Tools don't directly interact with the way output is handled but control other parts of the driver."
                new PaddingSpacerItem()
            };
        }
    }
}
