using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls
{
    public class HexBox : RestrictedTextBox<int>
    {
        public override int Value => int.TryParse(Regex.Replace(Text, "^0x", string.Empty), NumberStyles.HexNumber, null, out var value) ? value : 0;
        public override bool Restrictor(string str) => !IsHex(str);

        public static bool IsHex(string str)
        {
            if (str == string.Empty)
                return true;
            return int.TryParse(regex.Replace(str, string.Empty), NumberStyles.HexNumber, null, out var _);
        }

        private readonly static Regex regex = new Regex("^0x", RegexOptions.Compiled);
    }
}