using Eto.Forms;
using System;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Tools
{
    public static class UXTools
    {
        public static void RestrictToNumber(object _, TextChangingEventArgs args)
        {
            var text = Regex.Replace(args.Text, "\\.$", ".0");
            args.Cancel = !double.TryParse(text, out var _);
        }
    }
}