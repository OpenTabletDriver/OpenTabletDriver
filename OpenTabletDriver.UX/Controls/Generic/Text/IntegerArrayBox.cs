using System.Linq;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class IntegerArrayBox : MaskedTextBox<int[]>
    {
        public IntegerArrayBox()
        {
        }

        private class IntegerArrayTextProvider : NumberArrayTextProvider<int[]>
        {
            public override int[] Value
            {
                set => Text = string.Join(' ', value);
                get => Text.Split(' ').Select(str => int.TryParse(str, out var val) ? val : 0).ToArray();
            }
        }
    }
}