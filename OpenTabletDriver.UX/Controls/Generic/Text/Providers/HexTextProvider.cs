using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic.Text.Providers
{
    public abstract class HexTextProvider<T> : IMaskedTextProvider<T>
    {
        protected virtual Regex HexadecimalRegex => new Regex(SINGLE_HEXADECIMAL_REGEX);

        protected const string SINGLE_HEXADECIMAL_REGEX = $"^{HEXADECIMAL_REGEX}$";
        protected const string HEXADECIMAL_REGEX = "(?:0x)?(?:[0-9A-Fa-f]+[0-9A-Fa-f]?)?";
        protected const string VALID_CHAR_REGEX = @"[0-9A-Fa-f]";

        protected readonly StringBuilder builder = new StringBuilder();

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

        protected virtual bool Allow(ref char character, ref int position)
        {
            char[] characters = builder.ToString().ToCharArray();
            if (position == characters.Length)
                characters = characters.Append(character).ToArray();
            else
                characters[position] = character;
            return HexadecimalRegex.IsMatch(new string(characters));
        }
    }
}