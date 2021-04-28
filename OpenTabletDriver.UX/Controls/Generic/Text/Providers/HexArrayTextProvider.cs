using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class HexArrayTextProvider<T> : HexTextProvider<T> where T : IEnumerable
    {
        protected override Regex HexadecimalRegex => new Regex(HEXADECIMAL_REGEX);
    }
}