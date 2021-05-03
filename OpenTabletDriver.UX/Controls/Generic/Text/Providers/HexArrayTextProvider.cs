using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class HexArrayTextProvider<T> : HexTextProvider<T> where T : IEnumerable
    {
        protected override bool Allow(ref char character, ref int position)
        {
            var newValue = BuildString(ref character, ref position);
            var tokens = newValue.Split(' ', StringSplitOptions.TrimEntries);

            return tokens.All(Validate);
        }

        protected virtual bool Validate(string str)
        {
            return HexadecimalRegex.IsMatch(str);
        }
    }
}