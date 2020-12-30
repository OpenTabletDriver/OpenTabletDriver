using Eto.Drawing;
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
                new TextContent
                {
                    "Welcome to OpenTabletDriver!",
                    "OpenTabletDriver is an open source, cross platform, user mode tablet driver."
                },
                new PaddingSpacerItem(),
            };
        }
    }
}