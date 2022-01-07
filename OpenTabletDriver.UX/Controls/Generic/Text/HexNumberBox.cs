using System.Globalization;
using System.Text.RegularExpressions;
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

        private class HexTextProvider : RegexTextProvider<int>
        {
            protected override Regex Regex => new Regex(@"^(?:0x)?(?:[0-9A-F]{1,4})?$");

            public override int Value
            {
                set => Text = "0x" + value.ToString("X4");
                get => int.TryParse(Text.Replace("0x", string.Empty), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result) ? result : default(int);
            }
        }
    }
}
