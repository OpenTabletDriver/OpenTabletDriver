using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Attributes;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Greeter.Pages
{
    [PageName("Intro")]
    public class WelcomePage : StylizedPage
    {
        public WelcomePage()
        {
            this.Content = new StackedContent
            {
                new PaddingSpacerItem(),
                new Bitmap(App.Logo.WithSize(256, 256)),
                new StylizedText("OpenTabletDriver Guide", Fonts.Cached("Sans", 12, FontStyle.Bold), new Padding(0, 0, 0, 10)),
                "Welcome to OpenTabletDriver!",
                "OpenTabletDriver is an open source, cross platform, user mode tablet driver.",
                new PaddingSpacerItem(),
            };
        }
    }
}