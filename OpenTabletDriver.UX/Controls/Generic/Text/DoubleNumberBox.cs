using System.Globalization;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class DoubleNumberBox : MaskedTextBox<double>
    {
        public DoubleNumberBox()
        {
            Provider = new DoubleTextProvider();
        }

        private class DoubleTextProvider : NumberTextProvider<double>
        {
            public override double Value
            {
                set => Text = value.ToString(CultureInfo.InvariantCulture);
                get => double.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : default(double);
            }
        }
    }
}
