using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class FatalErrorDialog : Dialog
    {
        private readonly App _app;

        public FatalErrorDialog(App app, string error)
        {
            _app = app;

            Title = "OpenTabletDriver Fatal Error";

            Width = 400;
            Height = 300;

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(null, true),
                    new Bitmap(Metadata.Logo, 192, 192),
                    error,
                    new StackLayoutItem(null, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new Button(ShowWiki)
                            {
                                Text = "Help"
                            },
                            new Button((_, _) => Close())
                            {
                                Text = "Close"
                            }
                        }
                    }
                }
            };
        }

        private void ShowWiki(object? sender, EventArgs e)
        {
            _app.Open(Metadata.WIKI_URL);
            Close();
        }
    }
}
