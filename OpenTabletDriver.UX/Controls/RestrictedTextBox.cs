using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public abstract class RestrictedTextBox<T> : TextBox
    {
        public RestrictedTextBox()
        {
            TextChanging += (_, args) => args.Cancel = Restrictor(args.NewText);
        }

        public abstract T Value { get; }
        public abstract bool Restrictor(string str);
    }
}