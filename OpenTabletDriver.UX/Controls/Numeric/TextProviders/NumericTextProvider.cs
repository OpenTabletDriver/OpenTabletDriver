using System.Text;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Numeric.TextProviders
{
    public abstract class NumericTextProvider<T> : IMaskedTextProvider<T>
    {
        protected StringBuilder Builder { get; } = new StringBuilder();

        public abstract T Value { get; set; }

        public string DisplayText => Builder.ToString();

        public string? Text
        {
            set
            {
                Builder.Clear();
                if (value != null)
                {
                    var pos = 0;
                    foreach (var c in value)
                    {
                        Insert(c, ref pos);
                    }
                }
            }
            get => Builder.ToString();
        }

        public bool MaskCompleted => true;

        public IEnumerable<int> EditPositions => Enumerable.Range(0, Builder.Length);

        public bool IsEmpty => Builder.Length == 0;

        public bool Clear(ref int position, int length, bool forward)
        {
            return Delete(ref position, length, forward);
        }

        public bool Delete(ref int position, int length, bool forward)
        {
            if (Builder.Length == 0)
                return false;
            if (forward)
            {
                length = Math.Min(length, Builder.Length - position);
                Builder.Remove(position, length);
            }
            else if (position >= length)
            {
                Builder.Remove(position - length, length);
                position = Math.Max(0, position - length);
            }
            return true;
        }

        public bool Insert(char character, ref int position)
        {
            if (Allow(ref character, ref position))
            {
                Builder.Insert(position, character);
                position++;
                return true;
            }
            return false;
        }

        public bool Replace(char character, ref int position)
        {
            if (Allow(ref character, ref position))
            {
                if (position >= Builder.Length)
                    return Insert(character, ref position);
                Builder[position] = character;
                position++;
            }
            return true;
        }

        protected abstract bool Allow(ref char character, ref int position);
    }
}
