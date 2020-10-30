using System;
using System.Text.RegularExpressions;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX.Tools
{
    public class HexBox : RestrictedTextBox<int>
    {
        public override int Value
        {
            get
            {
                try
                {
                    return Convert.ToInt32(this.Text);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override bool Restrictor(string str) => StaticRestrictor(str);
        public static bool StaticRestrictor(string str)
        {
            if (str == "0" || str == "0x")
            {
                str = "0x0";
            }

            return !Regex.IsMatch(str, "^0x[0-9a-f]+$");
        }
    }
}