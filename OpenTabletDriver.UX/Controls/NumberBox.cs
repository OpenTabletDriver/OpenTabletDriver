using System.Text.RegularExpressions;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX.Tools
{
    public class NumberBox : RestrictedTextBox<double>
    {
        public override double Value => double.TryParse(this.Text, out var val) ? val : 0;
        public override bool Restrictor(string str) => StaticRestrictor(str);
        public static bool StaticRestrictor(string str) => !Regex.IsMatch(str, "^-*[0-9]*[\\.,]*[0-9]*$");
    }
}