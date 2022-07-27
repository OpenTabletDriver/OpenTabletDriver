using Eto.Forms;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Dialogs
{
    public class StringDialog : DesktopDialog<string?>
    {
        public StringDialog()
        {
            var tb = new TextBox();
            tb.TextBinding.Bind(DataContextBinding.Cast<string?>());

            tb.KeyDown += (_, e) =>
            {
                if (e.Key == Keys.Enter)
                    Close(DataContext as string);
            };

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
                            new Button((_, _) => Close(DataContext as string))
                            {
                                Text = "Ok"
                            }
                        }
                    }
                }
            };
        }
    }
}
