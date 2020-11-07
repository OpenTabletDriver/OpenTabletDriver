using System.Text.RegularExpressions;

namespace OpenTabletDriver.UX.Controls
{
    public class NumberBox : RestrictedTextBox<double>
    {
        public override double Value => double.TryParse(this.Text, out var val) ? val : 0;
        public override bool Restrictor(string str) => !IsNumber(str);
        public static bool IsNumber(string str) => regex.IsMatch(str);

        private readonly static Regex regex = new Regex("^-?[0-9]*[\\.,]?[0-9]*$", RegexOptions.Compiled);
    }
}