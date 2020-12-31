using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Windows.Greeter.Pages;

namespace OpenTabletDriver.UX.Windows.Greeter
{
    public class StartupGreeterWindow : Dialog
    {
        public StartupGreeterWindow()
        {
            base.Title = "OpenTabletDriver Guide";
            base.ClientSize = new Size(880, 680);
            base.Icon = App.Logo.WithSize(256, 256);
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
                    new BindingPage()
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

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
            }
        }
    }
}