using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls
{
    public class HexBox : RestrictedTextBox<int>
    {
        public override int Value => int.TryParse(Regex.Replace(Text, "^0x", string.Empty), NumberStyles.HexNumber, null, out var value) ? value : 0;
        public override bool Restrictor(string str) => StaticRestrictor(str);

        public static bool StaticRestrictor(string str)
        {
            if (str == string.Empty)
                return false;
            return !int.TryParse(Regex.Replace(str, "^0x", string.Empty), NumberStyles.HexNumber, null, out var _);
        }
    }
}