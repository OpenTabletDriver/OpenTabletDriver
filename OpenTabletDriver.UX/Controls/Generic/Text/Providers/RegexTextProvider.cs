using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class RegexTextProvider<T> : MaskedTextProvider<T>
    {
        protected abstract Regex Regex { get; }

        protected override bool Allow(ref char character, ref int position)
        {
            builder.Insert(position, character);
            var builderResult = builder.ToString();
            builder.Remove(position, 1);
            return Regex.IsMatch(builderResult);
        }
    }
}
