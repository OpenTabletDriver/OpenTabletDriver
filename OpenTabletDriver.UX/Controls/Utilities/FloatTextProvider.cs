using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Utilities
{
    public class FloatTextProvider : IMaskedTextProvider<float>
    {
        public float Value { set; get; }

        public string DisplayText => Value.ToString();

        public string Text { set; get; }

        public bool MaskCompleted => throw new System.NotImplementedException();

        public IEnumerable<int> EditPositions => throw new System.NotImplementedException();

        public bool IsEmpty => throw new System.NotImplementedException();

        public bool Clear(ref int position, int length, bool forward)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(ref int position, int length, bool forward)
        {
            throw new System.NotImplementedException();
        }

        public bool Insert(char character, ref int position)
        {
            throw new System.NotImplementedException();
        }

        public bool Replace(char character, ref int position)
        {
            throw new System.NotImplementedException();
        }
    }
}
