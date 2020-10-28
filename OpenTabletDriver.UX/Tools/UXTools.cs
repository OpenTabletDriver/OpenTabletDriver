using Eto.Forms;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Tools
{
    public static class UXTools
    {
        public static void RestrictToNumber(object _, TextChangingEventArgs args)
        {
            args.Cancel = !Regex.IsMatch(args.NewText, "^-*[0-9]*[\\.,]*[0-9]*$");
        }
    }
}