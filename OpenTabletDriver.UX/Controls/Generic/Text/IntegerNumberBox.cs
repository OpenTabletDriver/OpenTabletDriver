using System.Globalization;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class IntegerNumberBox : MaskedTextBox<int>
    {
        public IntegerNumberBox()
        {
            Provider = new IntegerTextProvider();
        }

        private class IntegerTextProvider : NumberTextProvider<int>
        {
            public override int Value
            {
                set => Text = value.ToString(CultureInfo.InvariantCulture);
                get => int.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : default(int);
            }
        }
    }
}
