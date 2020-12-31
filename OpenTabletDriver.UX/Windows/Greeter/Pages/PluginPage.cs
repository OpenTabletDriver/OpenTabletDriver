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
                new Bitmap(App.Logo.WithSize(256, 256)),
                "Plugins can be downloaded from the plugin manager at your own risk.",
                "The plugin manager can be found in the Plugins menu in the main window.",
                new PaddingSpacerItem()
            };
        }
    }
}