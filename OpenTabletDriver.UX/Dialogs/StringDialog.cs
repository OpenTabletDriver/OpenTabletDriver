using Eto.Forms;

namespace OpenTabletDriver.UX.Dialogs
{
    public class StringDialog : Dialog<string?>
    {
        public StringDialog()
        {
            var binding = new BindableBinding<StringDialog, string?>(
                this,
                c => c.DataContext as string,
                (d, s) => d.DataContext = s,
                (d, h) => d.DataContextChanged += h,
                (d, h) => d.DataContextChanged -= h
            );

            var tb = new TextBox();
            tb.TextBinding.Bind(binding);

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(tb, true),
                    new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 5,
                        Items =
                        {
                            new StackLayoutItem(null, true),
                            new Button((_, _) => Close())
                            {
                                Text = "Cancel"
                            },
                            new Button(Ok)
                            {
                                Text = "Ok"
                            }
                        }
                    }
                }
            };
        }

        private void Ok(object? sender, EventArgs e)
        {
            Close(DataContext as string);
        }
    }
}
