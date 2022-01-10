using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Eto.Forms;
using OpenTabletDriver.UX.Controls.Generic.Text.Providers;

namespace OpenTabletDriver.UX.Controls.Generic.Text
{
    public class HexByteArrayBox : MaskedTextBox<byte[]>
    {
        public HexByteArrayBox()
        {
            Provider = new HexByteArrayTextProvider();
        }

        private class HexByteArrayTextProvider : RegexTextProvider<byte[]>
        {
            protected override Regex Regex => new Regex(@"^(?:(?:(?:(?<=^| )0?(?<=0)x?)?(?:(?<=0x)[0-9A-F]{1,2})?)(?:(?<! ) ?))*$");

            public override byte[] Value
            {
                set => Text = ToHexString(value);
                get => ToByteArray(Text);
            }

            private bool TryGetHexValue(string str, out byte value) => byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);

            private string ToHexString(byte[] value)
            {
                if (value is byte[] array)
                    return "0x" + BitConverter.ToString(array).Replace("-", " 0x") ?? string.Empty;
                else
                    return string.Empty;
            }

            private byte[] ToByteArray(string hex)
            {
                var raw = hex.Split(' ');
                byte[] buffer = new byte[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                {
                    if (TryGetHexValue(raw[i], out var val))
                        buffer[i] = val;
                    else
                        return null;
                }
                return buffer;
            }
        }
    }
}
