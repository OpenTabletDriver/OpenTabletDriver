using System.Linq.Expressions;
using Eto.Forms;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.ViewModels;

namespace OpenTabletDriver.UX.Dialogs
{
    public sealed class PluginRepositoryDialog : DesktopDialog<RepositoryModel?>
    {
        public PluginRepositoryDialog()
        {
            Title = "Select a repository...";
            DataContext = new RepositoryModel();

            Width = 300;

            Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    ControlFor(m => m.Owner),
                    ControlFor(m => m.Name),
                    ControlFor(m => m.GitRef),
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
                            new Button((_, _) => Close(DataContext as RepositoryModel))
                            {
                                Text = "Ok"
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Creates a control from an expression.
        /// </summary>
        /// <param name="expression">The expression pointing to the target member.</param>
        private static Control ControlFor(Expression<Func<RepositoryModel, string>> expression)
        {
            var textBox = new TextBox();
            textBox.TextBinding.BindDataContext(expression);

            var title = expression.GetFriendlyName();
            return new LabeledGroup(title, textBox);
        }
    }
}
