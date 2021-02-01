using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class NumberTextProvider<T> : IMaskedTextProvider<T>
    {
        private const char DECIMAL_CHAR = '.';
        private readonly StringBuilder builder = new StringBuilder();
        
        public abstract T Value { get; set; }

        public virtual string DisplayText => builder.ToString();

        public virtual string Text
        {
            set
            {
                builder.Clear();
                if (value != null)
				{
					int pos = 0;
					foreach (char ch in value)
					{
						Insert(ch, ref pos);
					}
				}
            }
            get => builder.ToString();
        }

        public bool MaskCompleted => true;

        public IEnumerable<int> EditPositions
        {
            get
            {
                for (int i = 0; i <= builder.Length; i++)
				{
					yield return i;
				}
            }
        }

        public bool IsEmpty => builder.Length == 0;

        public bool Clear(ref int position, int length, bool forward)
        {
            return Delete(ref position, length, forward);
        }

        public bool Delete(ref int position, int length, bool forward)
        {
            if (builder.Length == 0)
				return false;
			if (forward)
			{
				length = Math.Min(length, builder.Length - position);
				builder.Remove(position, length);
			}
			else if (position >= length)
			{
				builder.Remove(position - length, length);
				position = Math.Max(0, position - length);
			}
			return true;
        }

        public bool Insert(char character, ref int position)
        {
            if (Allow(ref character, ref position))
            {
                builder.Insert(position, character);
                position++;
                return true;
            }
            return false;
        }

        public bool Replace(char character, ref int position)
        {
            if (Allow(ref character, ref position))
            {
                if (position >= builder.Length)
                    return Insert(character, ref position);
                builder[position] = character;
                position++;
            }
			return true;
        }

        private bool Allow(ref char character, ref int position)
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
