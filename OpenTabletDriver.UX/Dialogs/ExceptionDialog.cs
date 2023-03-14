using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class ExceptionDialog : Dialog
    {
        public ExceptionDialog(Exception exception)
        {
            Title = "Application Error";
            Width = 600;
            Height = 400;

            var headerLabel = new Panel
            {
                Padding = 10,
                Content = new Label
                {
                    Text = "An application error has occured. Report this to the developers!"
                }
            };

            var stackTrace = new TextArea
            {
                ReadOnly = true,
                Wrap = false,
                Text = exception.ToString()
            };

            var copyButton = new Button((_, _) => Clipboard.Instance.Text = stackTrace.Text)
            {
                Text = "Copy"
            };

            var okButton = new Button((_, _) => Close())
            {
                Text = "Ok"
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(headerLabel, HorizontalAlignment.Center),
                    new StackLayoutItem(stackTrace, HorizontalAlignment.Stretch, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            copyButton,
                            okButton
                        }
                    }
                }
            };
        }
    }
}
