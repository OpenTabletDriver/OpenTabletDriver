using System;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows.Greeter.Pages;

namespace OpenTabletDriver.UX.Windows.Greeter
{
    public class StartupGreeterWindow : DesktopDialog
    {
        public StartupGreeterWindow(Window parent)
            : base(parent)
        {
            base.Title = "OpenTabletDriver Guide";
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            var pageViewer = new StartupGreeterPageViewer
            {
                Pages =
                {
                    new WelcomePage(),
                    new AreaEditorPage(),
                    new BindingPage(),
                    new PluginPage(),
                    new BackgroundingPage(),
                    new FAQPage()
                }
            };

            var nextButton = new Button((sender, e) => pageViewer.NextPage())
            {
                Text = "Next"
            };

            var prevButton = new Button((sender, e) => pageViewer.PreviousPage())
            {
                Text = "Previous"
            };

            pageViewer.SelectedIndexChanged += (sender, e) =>
            {
                prevButton.Enabled = pageViewer.SelectedIndex > 0;
                nextButton.Enabled = pageViewer.SelectedIndex <= pageViewer.Pages.Count;
                nextButton.Text = pageViewer.SelectedIndex == pageViewer.Pages.Count - 1 ? "Close" : "Next";
            };

            base.Content = new StackedContent
            {
                new StackLayoutItem
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Expand = true,
                    Control = pageViewer
                },
                new StackLayoutItem
                {
                    Control = new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            prevButton,
                            nextButton
                        }
                    }
                }
            };
        }

        private class StartupGreeterPageViewer : DocumentControl
        {
            public void PreviousPage()
            {
                if (SelectedIndex > 0)
                    SelectedIndex--;
            }

            public void NextPage()
            {
                if (SelectedIndex < Pages.Count - 1)
                    SelectedIndex++;
                else if (SelectedIndex == Pages.Count - 1)
                    base.ParentWindow.Close();
            }
        }
    }
}
