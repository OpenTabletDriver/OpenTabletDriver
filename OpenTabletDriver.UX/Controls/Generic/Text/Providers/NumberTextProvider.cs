namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class NumberTextProvider<T> : MaskedTextProvider<T>
    {
        private const char DECIMAL_CHAR = '.';

        protected override bool Allow(ref char character, ref int position)
        {
            bool allow = false;
            if (!allow && character == DECIMAL_CHAR)
            {
                character = DECIMAL_CHAR;
                var decimalIndex = Text.IndexOf(DECIMAL_CHAR);

                if (decimalIndex >= 0)
                {
                    builder.Remove(decimalIndex, 1);
                    if (position > decimalIndex)
                        position--;
                }

                allow = true;
                if (position < builder.Length && !char.IsDigit(builder[position]))
                {
                    // insert at correct location and move cursor
                    int idx;
                    for (idx = 0; idx < builder.Length; idx++)
                    {
                        if (char.IsDigit(builder[idx]))
                        {
                            break;
                        }
                    }
                    position = idx;
                    allow = true;
                }
            }

            if (!allow && character == '-')
            {
                var val = Text;
                if (val.IndexOf('-') == 0)
                {
                    builder.Remove(0, 1);
                    if (position == 0)
                        position++;
                }
                else
                    position++;
                builder.Insert(0, character);
                return false;
            }
            allow |= char.IsDigit(character);
            return allow;
        }
    }
}
