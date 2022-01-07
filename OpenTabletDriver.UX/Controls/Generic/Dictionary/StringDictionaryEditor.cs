using System.Linq;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic.Dictionary
{
    public class StringDictionaryEditor : DictionaryEditor<string, string>
    {
        protected override Control CreateControl(DirectBinding<string> keyBinding, DirectBinding<string> valueBinding)
        {
            TextBox keyBox = new TextBox();
            keyBox.TextBinding.Bind(keyBinding);
            keyBox.TextChanging += (sender, e) => e.Cancel = ItemSource.Keys.Contains(e.NewText);

            TextBox valueBox = new TextBox();
            valueBox.TextBinding.Bind(valueBinding);

            return new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(0, 0, 5, 0),
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(keyBox, true),
                    new StackLayoutItem(valueBox, true)
                }
            };
        }

        protected override void AddNew() => Add(string.Empty, string.Empty);
    }
}
