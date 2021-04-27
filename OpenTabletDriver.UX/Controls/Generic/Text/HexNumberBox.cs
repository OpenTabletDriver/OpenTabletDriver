using System.Globalization;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class HexNumberBox : MaskedTextBox<int>
    {
        public HexNumberBox()
        {
            Provider = new HexTextProvider();
        }

        private class HexTextProvider : HexTextProvider<int>
        {
            public override int Value
            {
                set => Text = "0x" + value.ToString("X4");
                get => int.TryParse(Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result) ? result : default(int);
            }
        }
    }
}