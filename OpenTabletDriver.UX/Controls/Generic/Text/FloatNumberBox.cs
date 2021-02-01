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

        private class FloatTextProvider : NumberTextProvider<float>
        {
            public override float Value
            {
                set => Text = value.ToString();
                get => float.TryParse(Text, out var val) ? val : default(float);
            }
        }
    }
}
