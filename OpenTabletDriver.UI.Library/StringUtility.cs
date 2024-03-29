using System.Globalization;
using System.Text;

namespace OpenTabletDriver.UI;

internal static class StringUtility
{
    public static bool TryParseFloat(string? value, out float result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = 0;
            return true;
        }
        else
        {
            value = InsertImplicitZeroes(value);
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }

    public static bool TryParseDouble(string? value, out double result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = 0;
            return true;
        }
        else
        {
            value = InsertImplicitZeroes(value);
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }

    private static string InsertImplicitZeroes(string value)
    {
        var sb = new StringBuilder(value.Length + 3); // at most 3 characters will be added

        // Special cases
        if (value == "." || value == "-" || value == "-.")
        {
            return "0";
        }

        if (value.StartsWith("-."))
        {
            sb.Append("-0.");
            sb.Append(value.AsSpan(2));
        }
        else if (value.StartsWith("."))
        {
            sb.Append("0.");
            sb.Append(value.AsSpan(1));
        }
        else
        {
            sb.Append(value);
        }

        if (value.EndsWith("."))
        {
            sb.Append('0');
        }

        return sb.ToString();
    }
}
