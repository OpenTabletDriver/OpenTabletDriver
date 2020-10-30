using System.Text.RegularExpressions;
using Eto.Forms;

namespace OpenTabletDriver.UX.Tools
{
    public class NumberBox : TextBox
    {
        public NumberBox()
        {
            TextChanging += RestrictToNumber;
        }

        public static void RestrictToNumber(object _, TextChangingEventArgs args)
        {
            args.Cancel = !Regex.IsMatch(args.NewText, "^-*[0-9]*[\\.,]*[0-9]*$");
        }
    }
}