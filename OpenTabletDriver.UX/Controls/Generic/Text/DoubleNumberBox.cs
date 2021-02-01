using System;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class DoubleNumberBox : MaskedTextBox<double>
    {

        private class DoubleTextProvider : NumberTextProvider<double>
        {
            public override double Value
            {
                set => Text = value.ToString();
                get => double.TryParse(Text, out var val) ? val : default(double);
            }
        }
    }
}
