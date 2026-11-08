using System;
using System.Globalization;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class FloatNumberBox : MaskedTextBox<float>
    {

        public FloatNumberBox()
        {
            Provider = new FloatTextProvider();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta.Height == 0) return;

            bool isNegative = e.Delta.Height < 0;

            float multiplier = 1;

            if ((e.Modifiers & Keys.Shift) != 0)
                multiplier *= 2;

            if ((e.Modifiers & Keys.Control) != 0)
                multiplier *= 5;

            if ((e.Modifiers & Keys.Alt) != 0)
                multiplier /= 4;

            Provider.Value += (float)Math.Max(0.5, Math.Abs(e.Delta.Height) * multiplier) * (isNegative ? -1 : 1);

            UpdateText();
            UpdateBindings();
        }

        private class FloatTextProvider : NumberTextProvider<float>
        {
            public override float Value
            {
                set => Text = value.ToString(CultureInfo.InvariantCulture);
                get => float.TryParse(Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : default(float);
            }
        }
    }
}
