using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls.Numeric.TextProviders
{
    public abstract class RegexTextProvider<T> : NumericTextProvider<T>
    {
        protected abstract Regex Regex { get; }

        protected override bool Allow(ref char character, ref int position)
        {
            Builder.Insert(position, character);
            var builderResult = Builder.ToString();
            Builder.Remove(position, 1);
            return Regex.IsMatch(builderResult);
        }
    }
}
