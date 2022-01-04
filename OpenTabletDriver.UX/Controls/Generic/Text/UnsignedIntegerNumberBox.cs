using System.Globalization;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class UnsignedIntegerNumberBox : MaskedTextBox<uint>
    {
        public UnsignedIntegerNumberBox()
        {
            Provider = new UnsignedIntegerTextProvider();
        }

        private class UnsignedIntegerTextProvider : NumberTextProvider<uint>
        {
            public override uint Value
            {
                set => Text = value.ToString(CultureInfo.InvariantCulture);
                get => uint.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : default(uint);
            }
        }
    }
}
